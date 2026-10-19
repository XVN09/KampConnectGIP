"""Maakt van elke OpenAPI-spec een PDF met dezelfde opmaak.

Gebruik:  python build_pdfs.py
Vereist:  pip install pyyaml markdown pypdf playwright  +  playwright install chromium

Voor elke map 'NN. X Service/OpenAPI/XServiceOpenApiV<n>.yaml' wordt
'NN. X Service/PDF/XServiceApiV<n>.pdf' gemaakt (enkel voor de hoogste versie).
Een kopie van elke PDF komt ook in '00. Alle PDFs'.
"""
import base64, datetime, glob, html, io, os, re, shutil
import markdown, yaml
from playwright.sync_api import sync_playwright
from pypdf import PdfReader, PdfWriter

ROOT = os.path.dirname(os.path.abspath(__file__))
ASSETS = os.path.join(ROOT, '_assets')
ALL_PDFS = os.path.join(ROOT, '00. Alle PDFs')  # verzamelmap met alle PDF's
CHROMIUM = os.environ.get('CHROMIUM_PATH')  # optioneel

METHOD_ORDER = ['get', 'post', 'put', 'patch', 'delete']
MD = markdown.Markdown(extensions=['tables'])


def md(text):
    MD.reset()
    text = re.sub(r'(?m)^(?!\s*(?:\* |\||- ))(.+)\n(\s*\* )', r'\1\n\n\2', text or '')
    return MD.convert(text)


def esc(x):
    return html.escape(str(x))


def path_html(p):
    """Pad met afbreekpunten na elke '/', zodat lange paden netjes afbreken."""
    return esc(p).replace('/', '/<wbr>')


def b64(path):
    with open(path, 'rb') as f:
        return base64.b64encode(f.read()).decode()


# ───────────────────────── schema-hulpfuncties ─────────────────────────
def ref_name(ref):
    return ref.split('/')[-1]


def type_html(s):
    if s is None:
        return '—'
    if '$ref' in s:
        n = ref_name(s['$ref'])
        return f'<a class="ref" href="#schema-{n}">{n}</a>'
    if 'allOf' in s:
        return ' + '.join(type_html(x) for x in s['allOf'] if '$ref' in x) or 'object'
    t = s.get('type', 'object')
    if t == 'array':
        return f'array&lt;{type_html(s.get("items"))}&gt;'
    fmt = s.get('format')
    out = esc(t)
    if fmt:
        out += f' <span class="fmt">({esc(fmt)})</span>'
    return out


def notes_html(s):
    parts = []
    if not s:
        return ''
    if s.get('description'):
        parts.append(md(s['description']).replace('<p>', '').replace('</p>', ''))
    if 'enum' in s:
        parts.append('Waarden: ' + ', '.join(f'<code>{esc(v)}</code>' for v in s['enum']))
    rng = []
    for k, lbl in [('minimum', 'min'), ('maximum', 'max'), ('minLength', 'min. lengte'),
                   ('maxLength', 'max. lengte'), ('pattern', 'patroon')]:
        if k in s:
            rng.append(f'{lbl} <code>{esc(s[k])}</code>')
    if rng:
        parts.append(', '.join(rng))
    if 'default' in s:
        parts.append(f'Standaard: <code>{esc(s["default"])}</code>')
    if 'example' in s:
        ex = s['example']
        ex = ', '.join(map(str, ex)) if isinstance(ex, list) else ex
        parts.append(f'Vb. <code>{esc(ex)}</code>')
    if s.get('readOnly'):
        parts.append('Alleen-lezen')
    if s.get('nullable'):
        parts.append('Kan leeg zijn')
    return '<br>'.join(parts)


def flatten(s, schemas):
    """Geeft (properties, required) terug, met allOf opgelost."""
    props, req = {}, []
    if s is None:
        return props, req
    if '$ref' in s:
        return flatten(schemas.get(ref_name(s['$ref'])), schemas)
    for part in s.get('allOf', []):
        p, r = flatten(part, schemas)
        props.update(p); req += r
    props.update(s.get('properties', {}))
    req += s.get('required', [])
    return props, req


def fields_table(s, schemas):
    props, req = flatten(s, schemas)
    if not props:
        if s and s.get('additionalProperties'):
            return '<p class="muted">Vrij object (sleutel-waardeparen).</p>'
        return ''
    rows = ''.join(
        f'<tr><td class="mono">{esc(k)}</td><td>{type_html(v)}</td>'
        f'<td class="c">{"<span class=req>ja</span>" if k in req else "nee"}</td><td>{notes_html(v)}</td></tr>'
        for k, v in props.items())
    return ('<table class="fields"><colgroup><col style="width:24%"><col style="width:22%"><col style="width:10%"><col></colgroup>'
            '<thead><tr><th>Veld</th><th>Type</th><th class="c">Verplicht</th><th>Omschrijving</th></tr></thead>'
            f'<tbody>{rows}</tbody></table>')


def body_html(content, schemas):
    out = ''
    for ctype, media in content.items():
        s = media.get('schema', {})
        out += f'<div class="ctype">Content-Type: <code>{esc(ctype)}</code> · Schema: {type_html(s)}</div>'
        if '$ref' not in s and s.get('type') != 'array':
            out += fields_table(s, schemas)
        if media.get('examples'):
            for name, ex in media['examples'].items():
                val = yaml.dump(ex.get('value'), allow_unicode=True, sort_keys=False).strip()
                out += f'<div class="example"><div class="ex-title">Voorbeeld: {esc(name)}</div><pre>{esc(val)}</pre></div>'
    return out


# ───────────────────────── opbouw van het document ─────────────────────────
def parse_roles(desc):
    roles = []
    if desc:
        m = re.search(r'\*\*Rollen:\*\*\s*(.+)$', desc, re.M)
        if m:
            roles = re.findall(r'`([^`]+)`', m.group(1))
            desc = desc[:m.start()].rstrip()
    return desc, roles


def operations(spec):
    for path, item in spec['paths'].items():
        shared = item.get('parameters', [])
        for m in METHOD_ORDER:
            if m in item:
                yield path, m, item[m], shared


def resolve(obj, spec):
    if obj and '$ref' in obj:
        parts = obj['$ref'].lstrip('#/').split('/')
        cur = spec
        for p in parts:
            cur = cur[p]
        return cur
    return obj


def endpoint_html(path, method, op, shared, spec, schemas, idx):
    desc, roles = parse_roles(op.get('description'))
    public = op.get('security') == [] or (op.get('security') is None and not spec.get('security'))
    auth = '<span class="chip pub">Publiek</span>' if public else '<span class="chip auth">Bearer token</span>'
    chips = ''.join(f'<span class="chip role">{esc(r)}</span>' for r in roles)
    out = [f'<section class="endpoint" id="op-{idx}">',
           f'<div class="ep-head"><span class="method m-{method}">{method.upper()}</span>'
           f'<span class="path">{path_html(path)}</span></div>',
           f'<div class="ep-summary">{esc(op.get("summary", ""))}</div>',
           f'<div class="meta">{auth}{chips}</div>']
    if desc:
        out.append(f'<div class="desc">{md(desc)}</div>')
    params = [resolve(p, spec) for p in shared + op.get('parameters', [])]
    if params:
        rows = ''.join(
            f'<tr><td class="mono">{esc(p["name"])}</td><td>{esc(p["in"])}</td><td>{type_html(p.get("schema"))}</td>'
            f'<td class="c">{"<span class=req>ja</span>" if p.get("required") else "nee"}</td>'
            f'<td>{notes_html({**(p.get("schema") or {}), **({"description": p["description"]} if p.get("description") else {})})}</td></tr>'
            for p in params)
        out.append('<h4>Parameters</h4><table class="fields"><colgroup><col style="width:22%"><col style="width:9%">'
                   '<col style="width:20%"><col style="width:10%"><col></colgroup><thead><tr><th>Naam</th><th>In</th>'
                   f'<th>Type</th><th class="c">Verplicht</th><th>Omschrijving</th></tr></thead><tbody>{rows}</tbody></table>')
    if op.get('requestBody'):
        out.append('<h4>Request body</h4>' + body_html(op['requestBody'].get('content', {}), schemas))
    rows = ''
    for code, r in op.get('responses', {}).items():
        if code in ('401', '403') and '$ref' in r:
            continue  # staat eenmalig in het overzicht
        r = resolve(r, spec)
        content = r.get('content') or {}
        body = ', '.join(type_html(v.get('schema')) for v in content.values()) or '—'
        cls = 'ok' if str(code).startswith(('2', '3')) else 'err'
        rows += (f'<tr><td><span class="code {cls}">{esc(code)}</span></td><td>{md(r.get("description", "")).replace("<p>", "").replace("</p>", "")}</td>'
                 f'<td>{body}</td></tr>')
    out.append('<h4>Responses</h4><table class="fields"><colgroup><col style="width:11%"><col><col style="width:32%"></colgroup>'
               f'<thead><tr><th>Code</th><th>Omschrijving</th><th>Inhoud</th></tr></thead><tbody>{rows}</tbody></table>')
    out.append('</section>')
    return '\n'.join(out)


def build_html(spec, nr, name, version):
    info = spec['info']
    schemas = spec.get('components', {}).get('schemas', {})
    tags = spec.get('tags', [])
    ops = list(operations(spec))
    tag_names = [t['name'] for t in tags] + sorted({o[2]['tags'][0] for o in ops} - {t['name'] for t in tags})
    tag_desc = {t['name']: t.get('description', '') for t in tags}
    today = datetime.date.today().strftime('%d/%m/%Y')
    servers = ''.join(f'<div><code>{esc(s["url"])}</code> <span class="muted">— {esc(s.get("description", ""))}</span></div>'
                      for s in spec.get('servers', []))

    # nummering van endpoints
    numbered, i = {}, 0
    for t in tag_names:
        for o in ops:
            if o[2]['tags'][0] == t:
                i += 1; numbered[(o[0], o[1])] = i

    # overzichtstabel
    summary_rows = ''
    for t in tag_names:
        tops = [o for o in ops if o[2]['tags'][0] == t]
        if not tops:
            continue
        summary_rows += f'<tr class="grp"><td colspan="3">{esc(t)}</td></tr>'
        for path, m, op, _ in tops:
            summary_rows += (f'<tr><td><span class="method sm m-{m}">{m.upper()}</span></td>'
                             f'<td class="mono"><a href="#op-{numbered[(path, m)]}">{path_html(path)}</a></td><td>{esc(op.get("summary", ""))}</td></tr>')

    # standaardfouten
    std = spec.get('components', {}).get('responses', {})
    std_rows = ''.join(f'<tr><td class="mono">{esc(k)}</td><td>{esc(v.get("description", ""))}</td></tr>' for k, v in std.items())

    # endpoints per tag
    ep_html = ''
    for t in tag_names:
        tops = [o for o in ops if o[2]['tags'][0] == t]
        if not tops:
            continue
        ep_html += f'<h2 class="tag">{esc(t)}</h2>'
        if tag_desc.get(t):
            ep_html += f'<p class="tag-desc">{md(tag_desc[t]).replace("<p>", "").replace("</p>", "")}</p>'
        for path, m, op, shared in tops:
            ep_html += endpoint_html(path, m, op, shared, spec, schemas, numbered[(path, m)])

    # datamodellen
    sch_html = ''
    for n, s in schemas.items():
        sch_html += f'<section class="schema" id="schema-{n}"><h3>{esc(n)}</h3>'
        if s.get('description'):
            sch_html += f'<div class="desc">{md(s["description"])}</div>'
        if 'enum' in s:
            sch_html += '<div class="enum">' + ''.join(f'<code>{esc(v)}</code>' for v in s['enum']) + '</div>'
        else:
            sch_html += fields_table(s, schemas) or f'<p class="muted">Type: {type_html(s)}</p>'
        sch_html += '</section>'

    auth_text = ('Bearer token (JWT) van de Identity Service, tenzij een endpoint als <em>Publiek</em> gemarkeerd is.'
                 if spec.get('security') else 'Per endpoint aangegeven (publiek of bearer token).')

    return f'''<!doctype html><html lang="nl"><head><meta charset="utf-8"><title>{esc(name)} Service API V{version}</title>
<style>{css()}</style></head><body>
<!--COVER--><div class="cover">
  <div class="cover-top">
    <img class="logo" src="data:image/png;base64,{b64(os.path.join(ASSETS, 'logo.png'))}">
    <div class="kicker">API-documentatie</div>
    <div class="cover-nr">{nr}</div>
    <h1 class="cover-title">{esc(name)} Service</h1>
    <div class="cover-sub">OpenAPI {esc(spec["openapi"])} · Versie V{version}</div>
  </div>
  <div class="cover-bottom">
    <table class="cover-meta">
      <tr><td>Project</td><td>KampConnect</td></tr>
      <tr><td>Service</td><td>{nr}. {esc(name)} Service</td></tr>
      <tr><td>Versie</td><td>V{version} (spec {esc(info.get("version", ""))})</td></tr>
      <tr><td>Endpoints</td><td>{len(ops)}</td></tr>
      <tr><td>Datum</td><td>{today}</td></tr>
      <tr><td>Auteur</td><td>Xander Van Nuffel · 6ICW · GIP 2026-2027</td></tr>
    </table>
  </div>
</div><!--/COVER-->

<h1 class="chapter"><span>1</span>Overzicht</h1>
<div class="lead">{md(info.get("description", ""))}</div>
<div class="facts">
  <div><div class="lbl">Servers</div>{servers}</div>
  <div><div class="lbl">Authenticatie</div>{auth_text}
  {'<br><span class="muted">Elk beveiligd endpoint kan ook <code>401</code> (geen geldig token) of <code>403</code> (geen toegang) teruggeven; die staan niet bij elk endpoint herhaald.</span>' if spec.get('security') else ''}</div>
</div>
<h2>Endpoints</h2>
<table class="summary"><colgroup><col style="width:12%"><col style="width:42%"><col></colgroup>
<thead><tr><th>Methode</th><th>Pad</th><th>Omschrijving</th></tr></thead><tbody>{summary_rows}</tbody></table>


<h1 class="chapter newpage"><span>2</span>Endpoints</h1>
{ep_html}

<h1 class="chapter newpage"><span>3</span>Datamodellen</h1>
{f'<div class="keep"><h2 style="margin-top:0">Standaardresponses</h2><table class="fields"><colgroup><col style="width:28%"><col></colgroup><thead><tr><th>Naam</th><th>Omschrijving</th></tr></thead><tbody>{std_rows}</tbody></table></div>' if std_rows else ''}
<h2>Schema's</h2>
{sch_html}
</body></html>'''


def css():
    fonts = ''
    for fam, file, w in [('Poppins', 'poppins-latin-500-normal', 500), ('Poppins', 'poppins-latin-600-normal', 600),
                         ('Poppins', 'poppins-latin-700-normal', 700), ('Inter', 'inter-latin-400-normal', 400),
                         ('Inter', 'inter-latin-500-normal', 500), ('Inter', 'inter-latin-600-normal', 600),
                         ('Inter', 'inter-latin-700-normal', 700), ('JetBrains Mono', 'jetbrains-mono-latin-400-normal', 400),
                         ('JetBrains Mono', 'jetbrains-mono-latin-600-normal', 600)]:
        fonts += (f"@font-face{{font-family:'{fam}';font-weight:{w};src:url(data:font/woff2;base64,"
                  f"{b64(os.path.join(ASSETS, 'fonts', file + '.woff2'))}) format('woff2');}}\n")
    return fonts + '''
:root{--g1:#44946C;--g2:#408B66;--g3:#30694D;--g4:#214734;--g5:#6FF1B0;--g6:#5FCF97;
 --ink:#1d2b24;--muted:#66756d;--line:#dfe7e2;--soft:#f3f8f5;}
@page{size:A4;margin:20mm 17mm 20mm 17mm;}
*{box-sizing:border-box;}
body{font-family:'Inter','DejaVu Sans',sans-serif;font-size:9.4pt;line-height:1.5;color:var(--ink);margin:0;}
code,.mono,pre{font-family:'JetBrains Mono','DejaVu Sans Mono',monospace;}
code{font-size:8.3pt;background:var(--soft);border:1px solid var(--line);border-radius:3px;padding:0 3px;color:var(--g4);}
a{color:inherit;text-decoration:none;}
a.ref{color:var(--g3);font-weight:600;border-bottom:1px dotted var(--g1);}
.muted{color:var(--muted);}
h1,h2,h3,h4{font-family:'Poppins','DejaVu Sans',sans-serif;color:var(--g4);}

/* cover */
.cover{height:296.5mm;width:210mm;overflow:hidden;display:flex;flex-direction:column;page-break-after:always;}
.cover-top{flex:1;padding:26mm 22mm 0 22mm;}
.cover .logo{width:62mm;}
.kicker{margin-top:48mm;font-family:'Poppins';font-weight:600;font-size:11pt;letter-spacing:.18em;
 text-transform:uppercase;color:var(--g1);}
.cover-nr{font-family:'Poppins';font-weight:700;font-size:64pt;line-height:1;color:var(--g6);margin-top:8mm;}
.cover-title{font-size:34pt;line-height:1.1;margin:2mm 0 4mm 0;}
.cover-sub{font-size:11pt;color:var(--muted);}
.cover-bottom{background:var(--g4);color:#fff;padding:14mm 22mm 18mm 22mm;border-top:3mm solid var(--g5);}
.cover-meta{border-collapse:collapse;font-size:10pt;}
.cover-meta td{padding:1.6mm 0;border:0;background:transparent!important;}
.cover-meta td:first-child{color:var(--g5);width:34mm;font-weight:600;}

/* hoofdstukken */
.chapter{font-size:22pt;margin:0 0 5mm 0;padding-bottom:3mm;border-bottom:2px solid var(--g5);display:flex;align-items:baseline;gap:4mm;}
.chapter span{color:var(--g1);}
.newpage{page-break-before:always;}
h2{font-size:13.5pt;margin:7mm 0 2.5mm 0;}
h2.tag{font-size:15pt;margin-top:4mm;padding:2mm 0 1mm 0;border-bottom:1px solid var(--line);page-break-after:avoid;}
.tag-desc{color:var(--muted);margin:0 0 3mm 0;}
h3{font-size:11pt;margin:0 0 2mm 0;}
h4{font-size:8.5pt;text-transform:uppercase;letter-spacing:.06em;color:var(--g3);margin:2.5mm 0 1mm 0;page-break-after:avoid;}
.lead p{margin:0 0 2.2mm 0;}
.lead ul{margin:0 0 2.5mm 0;padding-left:5mm;}
.facts{display:grid;grid-template-columns:1fr 1fr;gap:4mm;margin:5mm 0;}
.facts>div{background:var(--soft);border:1px solid var(--line);border-left:3px solid var(--g1);border-radius:4px;padding:3mm 4mm;}
.lbl{font-size:7.5pt;text-transform:uppercase;letter-spacing:.08em;color:var(--g3);font-weight:700;margin-bottom:1mm;}

/* tabellen */
table{width:100%;border-collapse:collapse;table-layout:fixed;margin:1mm 0 2mm 0;}
th{background:var(--g4);color:#fff;text-align:left;font-weight:600;font-size:8pt;padding:1.6mm 2.2mm;}
td{border-bottom:1px solid var(--line);padding:1.5mm 2.2mm;vertical-align:top;overflow-wrap:break-word;}
tr{page-break-inside:avoid;}
tbody tr:nth-child(even) td{background:#fafcfb;}
.c{text-align:center;}
td.mono{font-size:8.2pt;}
.req{color:var(--g3);font-weight:700;}
.fmt{color:var(--muted);font-size:8pt;}
table.summary tr.grp td{background:var(--soft)!important;font-family:'Poppins';font-weight:600;color:var(--g3);padding-top:2.2mm;}
.lead table th{background:var(--g3);}

/* endpoints */
.method{display:inline-block;min-width:15mm;text-align:center;font-family:'JetBrains Mono';font-weight:600;font-size:8pt;
 color:#fff;border-radius:3px;padding:.6mm 2mm;}
.method.sm{min-width:13mm;font-size:7.2pt;padding:.3mm 1.5mm;}
.m-get{background:#2f7fb5;}.m-post{background:var(--g2);}.m-put{background:#b7791f;}.m-patch{background:#8a5cb8;}.m-delete{background:#c0392b;}
.endpoint{border:1px solid var(--line);border-radius:5px;padding:3mm 4mm 1.5mm 4mm;margin:0 0 4mm 0;page-break-inside:avoid;}
.ep-head{display:flex;align-items:center;gap:3mm;}
.path{font-family:'JetBrains Mono';font-weight:600;font-size:10pt;color:var(--g4);}
.ep-summary{font-family:'Poppins';font-weight:600;font-size:10.5pt;margin:1.8mm 0 1.2mm 0;}
.meta{margin-bottom:1mm;}
.chip{display:inline-block;font-size:7.4pt;font-weight:600;border-radius:10px;padding:.3mm 2.4mm;margin:0 1.5mm 1mm 0;}
.chip.auth{background:#e7eef6;color:#2f5f86;}.chip.pub{background:#fdf1dc;color:#8a5a12;}
.chip.role{background:#e3f6ec;color:var(--g3);font-family:'JetBrains Mono';}
.desc p{margin:0 0 1.5mm 0;}
.desc table{margin:2mm 0;}
.ctype{font-size:8.4pt;color:var(--muted);margin-bottom:1mm;}
.code{font-family:'JetBrains Mono';font-weight:600;font-size:8.2pt;padding:.2mm 1.6mm;border-radius:3px;}
.code.ok{background:#e3f6ec;color:var(--g3);}.code.err{background:#fbe9e7;color:#a83226;}
.example{margin:1.5mm 0;}
.ex-title{font-size:7.8pt;font-weight:600;color:var(--g3);}
pre{background:var(--g4);color:#e9fff3;font-size:7.8pt;padding:2.5mm 3mm;border-radius:4px;margin:.8mm 0;white-space:pre-wrap;}

/* schema's */
.keep{page-break-inside:avoid;}
.schema{margin:0 0 5mm 0;page-break-inside:avoid;}
.schema h3{border-left:3px solid var(--g1);padding-left:2.5mm;}
.enum code{display:inline-block;margin:0 1.5mm 1.5mm 0;}
'''


FOOTER = '''<div style="width:100%;font-family:'DejaVu Sans',sans-serif;font-size:7.5pt;color:#66756d;padding:0 17mm;
display:flex;justify-content:space-between;align-items:center;">
<span><b style="color:#30694D">KampConnect</b> &nbsp;·&nbsp; {nr}. {name} Service &nbsp;·&nbsp; API-documentatie V{version}</span>
<span>pagina <span class="pageNumber"></span> / <span class="totalPages"></span></span></div>'''


def main():
    jobs = []
    for d in sorted(glob.glob(os.path.join(ROOT, '[0-9][0-9]. * Service'))):
        folder = os.path.basename(d)
        nr, name = re.match(r'(\d\d)\. (.+) Service', folder).groups()
        specs = glob.glob(os.path.join(d, 'OpenAPI', '*OpenApiV*.yaml'))
        if not specs:
            continue
        latest = max(specs, key=lambda p: int(re.search(r'V(\d+)\.yaml$', p).group(1)))
        version = int(re.search(r'V(\d+)\.yaml$', latest).group(1))
        out = os.path.join(d, 'PDF', f'{name.replace(" ", "")}ServiceApiV{version}.pdf')
        jobs.append((latest, out, nr, name, version))

    with sync_playwright() as p:
        browser = p.chromium.launch(**({'executable_path': CHROMIUM} if CHROMIUM else {}))
        page = browser.new_page()
        for src, out, nr, name, version in jobs:
            with open(src, encoding='utf-8') as f:
                spec = yaml.safe_load(f)
            full = build_html(spec, nr, name, version)
            head, rest = full.split('<!--COVER-->')
            cover, body = rest.split('<!--/COVER-->')
            os.makedirs(os.path.dirname(out), exist_ok=True)
            parts = []
            for i, (content, footer) in enumerate([('<style>@page{margin:0}</style>' + cover, False), (body, True)]):
                page.set_content(head + content + '</body></html>' * (not footer), wait_until='load')
                page.evaluate('document.fonts.ready')
                parts.append(page.pdf(prefer_css_page_size=True, print_background=True, display_header_footer=footer,
                                      header_template='<span></span>',
                                      footer_template=FOOTER.format(nr=nr, name=name, version=version)))
            writer = PdfWriter()
            for data in parts:
                writer.append(PdfReader(io.BytesIO(data)))
            writer.add_metadata({'/Title': f'KampConnect — {name} Service API V{version}',
                                 '/Author': 'Xander Van Nuffel', '/Subject': 'OpenAPI-documentatie'})
            with open(out, 'wb') as f:
                writer.write(f)
            print('✓', os.path.relpath(out, ROOT))
            # kopie in de verzamelmap, met het servicenummer ervoor zodat de volgorde klopt
            os.makedirs(ALL_PDFS, exist_ok=True)
            shutil.copyfile(out, os.path.join(ALL_PDFS, f'{nr}. {os.path.basename(out)}'))
        browser.close()


if __name__ == '__main__':
    main()
