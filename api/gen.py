"""Genereert de OpenAPI 3.0.0-specs van KampConnect (services 02 t.e.m. 11).

Gebruik: python gen.py   (vereist pyyaml)
De Identity-spec (01) is met de hand geschreven en wordt niet door dit script aangemaakt.
"""
import copy, re, yaml, os

OUT = os.path.dirname(os.path.abspath(__file__))
GATEWAY = [{'url': 'https://api.kampconnect.be', 'description': 'Productie (via de API Gateway)'},
           {'url': 'https://localhost:5000', 'description': 'Lokaal (gateway)'}]

# Versie van de specs. Verhoog bij een nieuwe versie; de vorige bestanden blijven staan (zoals bij de databases).
VERSION = 1

# Nummering en volgorde zoals in de GIP-bundel (map Databases).
SERVICES = {
    'identity':     ('01', 'Identity'),
    'tenant':       ('02', 'Tenant'),
    'user':         ('03', 'User'),
    'camp':         ('04', 'Camp'),
    'messaging':    ('05', 'Messaging'),
    'calling':      ('06', 'Calling'),
    'billing':      ('07', 'Billing'),
    'payment':      ('08', 'Payment'),
    'notification': ('09', 'Notification'),
    'compliance':   ('10', 'Compliance'),
    'media':        ('11', 'Media'),
}

def service_path(filename, version=None):
    """'tenant-service.openapi.yaml' -> '02. Tenant Service/OpenAPI/TenantServiceOpenApiV1.yaml'"""
    key = filename.split('-service')[0]
    nr, name = SERVICES[key]
    return os.path.join(OUT, f'{nr}. {name} Service', 'OpenAPI', f'{name}ServiceOpenApiV{version or VERSION}.yaml')

# ───────── helpers ─────────
def S(t, **kw):
    base = {
        'string': {'type': 'string'}, 'text': {'type': 'string'},
        'uuid': {'type': 'string', 'format': 'uuid'},
        'int': {'type': 'integer', 'format': 'int32'},
        'long': {'type': 'integer', 'format': 'int64'},
        'bool': {'type': 'boolean'},
        'datetime': {'type': 'string', 'format': 'date-time'},
        'date': {'type': 'string', 'format': 'date'},
        'time': {'type': 'string', 'example': '22:00'},
        'email': {'type': 'string', 'format': 'email'},
        'uri': {'type': 'string', 'format': 'uri'},
        'number': {'type': 'number'},
    }[t]
    d = dict(base); d.update(kw); return d

def E(*values, **kw):
    d = {'type': 'string', 'enum': list(values)}; d.update(kw); return d

def R(name):
    return {'$ref': f'#/components/schemas/{name}'}

def A(item):
    return {'type': 'array', 'items': item}

def O(required=None, desc=None, **props):
    d = {'type': 'object'}
    if desc: d['description'] = desc
    if required: d['required'] = required
    d['properties'] = {k: (v if isinstance(v, dict) else S(v)) for k, v in props.items()}
    return d

def paged(name):
    return {'allOf': [R('Page'), O(items=A(R(name)))]}

def P(name, where='query', t='string', desc=None, required=None, **kw):
    s = t if isinstance(t, dict) else S(t, **kw)
    d = {'name': name, 'in': where, 'required': where == 'path' if required is None else required, 'schema': s}
    if desc: d['description'] = desc
    return d

PAGE_PARAMS = [P('page', t='int', minimum=1, default=1), P('pageSize', t='int', minimum=1, maximum=100, default=25)]

def op(tag, summary, desc=None, roles=None, params=None, body=None, ok=('200', None, 'OK'),
       public=False, errors=(), content_type='application/json'):
    o = {'tags': [tag], 'summary': summary}
    lines = []
    if desc: lines.append(desc.strip())
    if roles: lines.append('**Rollen:** ' + ', '.join(f'`{r}`' for r in roles))
    if lines: o['description'] = '\n\n'.join(lines)
    if public: o['security'] = []
    if params: o['parameters'] = params
    if body is not None:
        o['requestBody'] = {'required': True, 'content': {content_type: {'schema': body}}}
    code, schema, text = ok
    resp = {'description': text}
    if schema is not None:
        resp['content'] = {'application/json': {'schema': schema}}
    o['responses'] = {code: resp}
    errs = list(errors)
    if body is not None and '400' not in errs: errs.insert(0, '400')
    if not public: errs += ['401', '403']
    for e in errs:
        o['responses'][e] = {'$ref': '#/components/responses/' + ERR[e]}
    return o

ERR = {'400': 'ValidationError', '401': 'Unauthorized', '403': 'Forbidden', '404': 'NotFound', '409': 'Conflict'}

COMMON_COMPONENTS = {
    'securitySchemes': {'bearerAuth': {
        'type': 'http', 'scheme': 'bearer', 'bearerFormat': 'JWT',
        'description': 'Access token van de Identity Service. De gateway valideert het token, de service controleert tenant en rol.'}},
    'responses': {
        'ValidationError': {'description': 'Validatiefout', 'content': {'application/problem+json': {'schema': R('ProblemDetails')}}},
        'Unauthorized': {'description': 'Geen of ongeldig access token'},
        'Forbidden': {'description': 'Geen toegang (verkeerde rol of andere organisatie)', 'content': {'application/problem+json': {'schema': R('ProblemDetails')}}},
        'NotFound': {'description': 'Niet gevonden', 'content': {'application/problem+json': {'schema': R('ProblemDetails')}}},
        'Conflict': {'description': 'Conflict met de huidige toestand', 'content': {'application/problem+json': {'schema': R('ProblemDetails')}}},
    },
    'schemas': {
        'ProblemDetails': O(desc='Foutformaat volgens RFC 7807 (standaard in ASP.NET Core)',
                            type='string', title='string', status='int', detail='string',
                            errors={'type': 'object', 'additionalProperties': A(S('string'))}),
        'Page': O(['page', 'pageSize', 'totalCount'], page=S('int', example=1), pageSize=S('int', example=25), totalCount=S('int', example=3)),
    },
}

COMMON_DESC = """
**Algemene afspraken**
* Alle requests lopen via de API Gateway en dragen een `Authorization: Bearer <access token>`.
* De organisatie volgt uit de claim `tenant_id` in het token; ze staat daarom niet in de URL.
  Een token zonder `tenant_id` krijgt geen toegang tot organisatiegegevens, tenzij `is_platform_admin` aanwezig is.
* Tijdstippen zijn UTC in ISO 8601. Fouten volgen RFC 7807 (Problem Details).
"""

def spec(filename, title, desc, tags, paths, schemas, servers=GATEWAY, extra_components=None, common=True):
    comps = copy.deepcopy(COMMON_COMPONENTS)
    comps['schemas'].update(schemas)
    if extra_components:
        for k, v in extra_components.items(): comps.setdefault(k, {}).update(v)
    doc = {
        'openapi': '3.0.0',
        'info': {'title': f'KampConnect — {title}', 'version': '1.0.0',
                 'description': desc.strip() + ('\n' + COMMON_DESC if common else '')},
        'servers': servers,
        'security': [{'bearerAuth': []}],
        'tags': [{'name': n, 'description': d} for n, d in tags],
        'paths': paths,
        'components': comps,
    }
    path = service_path(filename)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, 'w', encoding='utf-8') as f:
        f.write(yaml.dump(doc, sort_keys=False, allow_unicode=True, width=110, default_flow_style=False))
    return filename

def ID(name, t='uuid'):
    return P(name, 'path', t)

FILES = []

# ═════════════════════════ 02 Tenant ═════════════════════════
T = 'Organisatie'; M = 'Leden & uitnodigingen'; PA = 'Platformbeheer'
FILES.append(spec('tenant-service.openapi.yaml', 'Tenant Service', """
Beheert de organisaties (tenants) die KampConnect gebruiken: gegevens, instellingen, domeinen,
functies en wie er lid is. Database: `tnt_*` (PostgreSQL).
""", [(T, 'De eigen organisatie van de ingelogde gebruiker'), (M, 'Lidmaatschappen en uitnodigingen'),
      (PA, 'Enkel voor beheerders van KampConnect (`is_platform_admin`)')], {
    '/api/tenants': {
        'get': op(PA, 'Alle organisaties opvragen', roles=['platform_admin'],
                  params=[P('status', t=R('TenantStatus'))] + PAGE_PARAMS, ok=('200', paged('Tenant'), 'OK')),
        'post': op(PA, 'Organisatie aanmaken', 'Publiceert `tenant.TenantCreated.v1`.', roles=['platform_admin'],
                   body=R('TenantCreate'), ok=('201', R('Tenant'), 'Aangemaakt'), errors=['409']),
    },
    '/api/tenants/{tenantId}/status': {
        'put': op(PA, 'Organisatie opschorten of heractiveren',
                  'Bij `suspended` wordt `tenant.TenantSuspended.v1` gepubliceerd.', roles=['platform_admin'],
                  params=[ID('tenantId', 'int')], body=O(['status'], status=R('TenantStatus'), reason='string'),
                  ok=('204', None, 'Gewijzigd'), errors=['404']),
    },
    '/api/tenants/{tenantId}/features/{featureKey}': {
        'put': op(PA, 'Functie aan- of uitzetten', roles=['platform_admin'],
                  params=[ID('tenantId', 'int'), P('featureKey', 'path', example='emergency_calling')],
                  body=O(['isEnabled'], isEnabled='bool'), ok=('204', None, 'Gewijzigd'), errors=['404']),
    },
    '/api/tenants/current': {
        'get': op(T, 'Mijn organisatie opvragen', roles=['guardian', 'staff'], ok=('200', R('Tenant'), 'OK')),
        'patch': op(T, 'Organisatiegegevens wijzigen', roles=['organisation_admin'], body=R('TenantUpdate'),
                    ok=('200', R('Tenant'), 'OK')),
    },
    '/api/tenants/current/settings': {
        'get': op(T, 'Instellingen opvragen', roles=['staff'], ok=('200', R('TenantSettings'), 'OK')),
        'put': op(T, 'Instellingen wijzigen', 'Publiceert `tenant.SettingsUpdated.v1`.', roles=['organisation_admin'],
                  body=R('TenantSettings'), ok=('200', R('TenantSettings'), 'OK')),
    },
    '/api/tenants/current/features': {
        'get': op(T, 'Actieve functies opvragen', roles=['guardian', 'staff'], ok=('200', A(R('Feature')), 'OK')),
    },
    '/api/tenants/current/domains': {
        'get': op(T, 'Domeinen opvragen', roles=['organisation_admin'], ok=('200', A(R('Domain')), 'OK')),
        'post': op(T, 'Domein toevoegen', 'Het domein moet daarna geverifieerd worden (DNS TXT-record).',
                   roles=['organisation_admin'], body=O(['domain'], domain=S('string', example='chiro-heist.be')),
                   ok=('201', R('Domain'), 'Toegevoegd'), errors=['409']),
    },
    '/api/tenants/current/domains/{domainId}': {
        'delete': op(T, 'Domein verwijderen', roles=['organisation_admin'], params=[ID('domainId', 'int')],
                     ok=('204', None, 'Verwijderd'), errors=['404']),
    },
    '/api/tenants/current/members': {
        'get': op(M, 'Leden opvragen', roles=['staff'], params=[P('role', t=R('RoleKey'))] + PAGE_PARAMS,
                  ok=('200', paged('Membership'), 'OK')),
    },
    '/api/tenants/current/members/{membershipId}': {
        'patch': op(M, 'Rol of status van een lid wijzigen', roles=['organisation_admin'],
                    params=[ID('membershipId', 'int')],
                    body=O(roleKey=R('RoleKey'), status=R('MembershipStatus')),
                    ok=('200', R('Membership'), 'OK'), errors=['404']),
        'delete': op(M, 'Lid verwijderen uit de organisatie', roles=['organisation_admin'],
                     params=[ID('membershipId', 'int')], ok=('204', None, 'Verwijderd'), errors=['404']),
    },
    '/api/tenants/current/invitations': {
        'get': op(M, 'Openstaande uitnodigingen', roles=['organisation_admin'], ok=('200', A(R('Invitation')), 'OK')),
        'post': op(M, 'Iemand uitnodigen',
                   'De uitnodigingslink bepaalt de rol. Publiceert `tenant.InvitationSent.v1`; Notification verstuurt de mail.',
                   roles=['organisation_admin'], body=R('InvitationCreate'), ok=('201', R('Invitation'), 'Verstuurd')),
    },
    '/api/tenants/current/invitations/{invitationId}': {
        'delete': op(M, 'Uitnodiging intrekken', roles=['organisation_admin'], params=[ID('invitationId', 'int')],
                     ok=('204', None, 'Ingetrokken'), errors=['404']),
    },
    '/api/invitations/{token}': {
        'get': op(M, 'Uitnodiging bekijken (publiek)',
                  'Toont naam van de organisatie en de rol vóór de registratie. Het token gaat daarna mee naar `POST /api/account/register` van de Identity Service.',
                  public=True, params=[P('token', 'path')], ok=('200', R('InvitationPreview'), 'OK'), errors=['404']),
    },
}, {
    'TenantStatus': E('onboarding', 'active', 'suspended', 'closed'),
    'RoleKey': E('organisation_admin', 'staff', 'guardian'),
    'MembershipStatus': E('active', 'inactive'),
    'Tenant': O(['tenantId', 'name', 'slug', 'status'], tenantId=S('int', example=12), name=S('string', example='Chiro Heist'),
                slug=S('string', example='chiro-heist', description='Subdomein: `chiro-heist.kampconnect.be`'),
                status=R('TenantStatus'), primaryEmail='email', primaryPhone='string', createdAt='datetime'),
    'TenantCreate': O(['name', 'slug', 'primaryEmail'], name='string', slug=S('string', pattern='^[a-z0-9-]{3,80}$'),
                      primaryEmail='email', primaryPhone='string'),
    'TenantUpdate': O(name='string', primaryEmail='email', primaryPhone='string'),
    'TenantSettings': O(timezone=S('string', example='Europe/Brussels'), emergencyCallingEnabled='bool',
                        dataRetentionDays=S('int', example=365)),
    'Feature': O(featureKey=S('string', example='emergency_calling'), isEnabled='bool'),
    'Domain': O(domainId='int', domain=S('string', example='chiro-heist.be'), isVerified='bool',
                verificationRecord=S('string', description='TXT-record om toe te voegen')),
    'Membership': O(membershipId='int', userId=S('int', description='ID uit de Identity Service'),
                    roleKey=R('RoleKey'), status=R('MembershipStatus'), createdAt='datetime'),
    'InvitationCreate': O(['email', 'roleKey'], email='email', roleKey=R('RoleKey')),
    'Invitation': O(invitationId='int', email='email', roleKey=R('RoleKey'), expiresAt='datetime', createdAt='datetime'),
    'InvitationPreview': O(tenantName='string', roleKey=R('RoleKey'), expiresAt='datetime'),
}))

# ═════════════════════════ 03 User ═════════════════════════
U = 'Profielen'; G = 'Ouder-kindkoppelingen'; C = 'Contacten & adressen'
FILES.append(spec('user-service.openapi.yaml', 'User Service', """
Wie iemand is binnen een organisatie: profielen, ouder-kindkoppelingen, noodcontacten en adressen.
Auth bezit de credentials, User bezit de persoon. Een kind is een profiel **zonder** login. Database: `usr_*`.
""", [(U, 'Profielgegevens'), (G, 'Relaties tussen ouders/voogden en kinderen'), (C, 'Noodcontacten en adressen')], {
    '/api/users/me': {
        'get': op(U, 'Mijn profiel', roles=['guardian', 'staff'], ok=('200', R('User'), 'OK')),
        'patch': op(U, 'Mijn profiel wijzigen', 'Publiceert `user.UserProfileUpdated.v1`.', roles=['guardian', 'staff'],
                    body=R('UserUpdate'), ok=('200', R('User'), 'OK')),
    },
    '/api/users': {
        'get': op(U, 'Profielen zoeken binnen de organisatie', roles=['staff'],
                  params=[P('search', desc='Zoekt op naam')] + PAGE_PARAMS, ok=('200', paged('User'), 'OK')),
    },
    '/api/users/{userId}': {
        'get': op(U, 'Profiel opvragen', 'Een ouder ziet enkel zichzelf en de eigen kinderen.', roles=['guardian', 'staff'],
                  params=[ID('userId', 'int')], ok=('200', R('User'), 'OK'), errors=['404']),
        'patch': op(U, 'Profiel wijzigen (bv. van een kind)', roles=['guardian', 'staff'],
                    params=[ID('userId', 'int')], body=R('UserUpdate'), ok=('200', R('User'), 'OK'), errors=['404']),
    },
    '/api/users/me/children': {
        'get': op(G, 'Mijn kinderen', roles=['guardian'], ok=('200', A(R('User')), 'OK')),
        'post': op(G, 'Kind toevoegen',
                   'Maakt een profiel zonder login aan en koppelt het meteen aan de ouder. Publiceert `user.UserProfileCreated.v1` en `user.GuardianLinked.v1`.',
                   roles=['guardian'], body=R('ChildCreate'), ok=('201', R('User'), 'Aangemaakt')),
    },
    '/api/users/{userId}/guardians': {
        'get': op(G, 'Ouders/voogden van een kind', roles=['guardian', 'staff'], params=[ID('userId', 'int')],
                  ok=('200', A(R('GuardianLink')), 'OK'), errors=['404']),
    },
    '/api/guardian-links': {
        'post': op(G, 'Extra ouder/voogd koppelen', 'Bv. de tweede ouder. Wordt actief na verificatie door de organisatie.',
                   roles=['guardian', 'staff'], body=R('GuardianLinkCreate'), ok=('201', R('GuardianLink'), 'Aangemaakt'),
                   errors=['409']),
    },
    '/api/guardian-links/{guardianLinkId}/verify': {
        'post': op(G, 'Koppeling verifiëren', 'Publiceert `user.GuardianLinked.v1`.', roles=['staff'],
                   params=[ID('guardianLinkId', 'int')], ok=('200', R('GuardianLink'), 'OK'), errors=['404']),
    },
    '/api/guardian-links/{guardianLinkId}': {
        'delete': op(G, 'Koppeling intrekken', roles=['guardian', 'staff'], params=[ID('guardianLinkId', 'int')],
                     ok=('204', None, 'Ingetrokken'), errors=['404']),
    },
    '/api/users/{userId}/emergency-contacts': {
        'get': op(C, 'Noodcontacten opvragen', roles=['guardian', 'staff'], params=[ID('userId', 'int')],
                  ok=('200', A(R('EmergencyContact')), 'OK'), errors=['404']),
        'post': op(C, 'Noodcontact toevoegen', roles=['guardian'], params=[ID('userId', 'int')],
                   body=R('EmergencyContactInput'), ok=('201', R('EmergencyContact'), 'Toegevoegd'), errors=['404']),
    },
    '/api/emergency-contacts/{emergencyContactId}': {
        'put': op(C, 'Noodcontact wijzigen', roles=['guardian'], params=[ID('emergencyContactId', 'int')],
                  body=R('EmergencyContactInput'), ok=('200', R('EmergencyContact'), 'OK'), errors=['404']),
        'delete': op(C, 'Noodcontact verwijderen', roles=['guardian'], params=[ID('emergencyContactId', 'int')],
                     ok=('204', None, 'Verwijderd'), errors=['404']),
    },
    '/api/users/{userId}/addresses': {
        'get': op(C, 'Adressen opvragen', roles=['guardian', 'staff'], params=[ID('userId', 'int')],
                  ok=('200', A(R('Address')), 'OK'), errors=['404']),
        'post': op(C, 'Adres toevoegen', roles=['guardian', 'staff'], params=[ID('userId', 'int')],
                   body=R('AddressInput'), ok=('201', R('Address'), 'Toegevoegd'), errors=['404']),
    },
    '/api/addresses/{addressId}': {
        'put': op(C, 'Adres wijzigen', roles=['guardian', 'staff'], params=[ID('addressId', 'int')],
                  body=R('AddressInput'), ok=('200', R('Address'), 'OK'), errors=['404']),
        'delete': op(C, 'Adres verwijderen', roles=['guardian', 'staff'], params=[ID('addressId', 'int')],
                     ok=('204', None, 'Verwijderd'), errors=['404']),
    },
}, {
    'User': O(['userId', 'givenName', 'familyName', 'status'], userId=S('int', example=58),
              givenName=S('string', example='Lotte'), familyName=S('string', example='Peeters'), birthDate='date',
              email='email', phone='string', preferredLanguage=S('string', example='nl'),
              avatarFileId=S('uuid', description='Bestand in de Media Service'),
              hasLogin=S('bool', description='false voor kinderen'), status=E('active', 'inactive', 'anonymized')),
    'UserUpdate': O(givenName='string', familyName='string', birthDate='date', phone='string',
                    preferredLanguage='string', avatarFileId='uuid'),
    'ChildCreate': O(['givenName', 'familyName', 'birthDate'], givenName='string', familyName='string', birthDate='date',
                     relationshipType=R('RelationshipType')),
    'RelationshipType': E('parent', 'legal_guardian', 'foster_parent', 'other'),
    'GuardianLink': O(guardianLinkId='int', guardianUserId='int', childUserId='int', relationshipType=R('RelationshipType'),
                      hasLegalAuthority='bool', status=E('pending', 'active', 'revoked'), verifiedAt='datetime'),
    'GuardianLinkCreate': O(['guardianEmail', 'childUserId', 'relationshipType'], guardianEmail='email',
                            childUserId='int', relationshipType=R('RelationshipType'), hasLegalAuthority='bool'),
    'EmergencyContactInput': O(['fullName', 'phone', 'priority'], fullName='string', relationshipType='string',
                               phone='string', email='email', priority=S('int', minimum=1, example=1),
                               authorizedForPickup=S('bool', description='Mag het kind ophalen')),
    'EmergencyContact': {'allOf': [O(emergencyContactId='int'), R('EmergencyContactInput')]},
    'AddressInput': O(['street', 'houseNumber', 'postalCode', 'city', 'countryCode'], addressType=E('home', 'billing', 'other'),
                      street='string', houseNumber='string', box='string', postalCode=S('string', example='2220'),
                      city=S('string', example='Heist-op-den-Berg'), countryCode=S('string', example='BE'), isPrimary='bool'),
    'Address': {'allOf': [O(addressId='int'), R('AddressInput')]},
}))

# ═════════════════════════ 04 Camp ═════════════════════════
K = 'Kampen'; GR = 'Groepen'; D = 'Deelnemers & leiding'; AC = 'Activiteiten'
FILES.append(spec('camp-service.openapi.yaml', 'Camp Service', """
Het kamp als organisatorische eenheid: kampen, groepen, deelnemers en leiding. Camp bepaalt **wie met wie**
mag communiceren; Messaging en Calling bouwen hun rechten op uit de `camp.*`-events. Geen inschrijvingen of betalingen.
Database: `cmp_*`.

Levenscyclus: `planned` → `active` → `ended` → `archived` (los daarvan: gepubliceerd ja/nee).
""", [(K, 'Kampen en hun status'), (GR, 'Groepsindeling binnen een kamp'), (D, 'Kinderen en leiding toewijzen'),
      (AC, 'Programma van een kamp')], {
    '/api/camps': {
        'get': op(K, 'Kampen opvragen', 'Een ouder ziet enkel kampen waar een van zijn kinderen aan deelneemt.',
                  roles=['guardian', 'staff'], params=[P('status', t=R('CampStatus'))] + PAGE_PARAMS,
                  ok=('200', paged('Camp'), 'OK')),
        'post': op(K, 'Kamp aanmaken',
                   'Controleert synchroon bij Tenant of de formule nog een kamp toelaat. Publiceert `camp.CampCreated.v1`.',
                   roles=['organisation_admin', 'staff'], body=R('CampInput'), ok=('201', R('Camp'), 'Aangemaakt'),
                   errors=['409']),
    },
    '/api/camps/{campId}': {
        'get': op(K, 'Kamp opvragen', roles=['guardian', 'staff'], params=[ID('campId')], ok=('200', R('Camp'), 'OK'), errors=['404']),
        'put': op(K, 'Kamp wijzigen', roles=['organisation_admin', 'staff'], params=[ID('campId')], body=R('CampInput'),
                  ok=('200', R('Camp'), 'OK'), errors=['404']),
        'delete': op(K, 'Kamp verwijderen', 'Enkel mogelijk zolang het kamp niet gepubliceerd is.',
                     roles=['organisation_admin'], params=[ID('campId')], ok=('204', None, 'Verwijderd'), errors=['404', '409']),
    },
    '/api/camps/{campId}/publish': {
        'post': op(K, 'Kamp publiceren', 'Publiceert `camp.CampPublished.v1`.', roles=['organisation_admin', 'staff'],
                   params=[ID('campId')], ok=('200', R('Camp'), 'OK'), errors=['404', '409']),
    },
    '/api/camps/{campId}/status': {
        'put': op(K, 'Status wijzigen', 'Enkel de volgende stap in de levenscyclus is toegelaten. Bij `ended` volgt `camp.CampEnded.v1`.',
                  roles=['organisation_admin', 'staff'], params=[ID('campId')], body=O(['status'], status=R('CampStatus')),
                  ok=('200', R('Camp'), 'OK'), errors=['404', '409']),
    },
    '/api/camps/{campId}/groups': {
        'get': op(GR, 'Groepen van een kamp', roles=['guardian', 'staff'], params=[ID('campId')], ok=('200', A(R('Group')), 'OK'), errors=['404']),
        'post': op(GR, 'Groep toevoegen', 'Messaging maakt automatisch een groepschat aan.', roles=['staff'],
                   params=[ID('campId')], body=R('GroupInput'), ok=('201', R('Group'), 'Aangemaakt'), errors=['404']),
    },
    '/api/camps/{campId}/groups/{groupId}': {
        'put': op(GR, 'Groep wijzigen', roles=['staff'], params=[ID('campId'), ID('groupId')], body=R('GroupInput'),
                  ok=('200', R('Group'), 'OK'), errors=['404']),
        'delete': op(GR, 'Groep verwijderen', 'Enkel als er geen actieve deelnemers meer in zitten.', roles=['staff'],
                     params=[ID('campId'), ID('groupId')], ok=('204', None, 'Verwijderd'), errors=['404', '409']),
    },
    '/api/camps/{campId}/participants': {
        'get': op(D, 'Deelnemers opvragen', roles=['staff'], params=[ID('campId'), P('groupId', t='uuid')],
                  ok=('200', A(R('Participant')), 'OK'), errors=['404']),
        'post': op(D, 'Kind toevoegen aan een groep',
                   'Controleert synchroon bij User of het kind bestaat en de ouder gemachtigd is. Publiceert `camp.ParticipantAdded.v1`.',
                   roles=['staff'], params=[ID('campId')], body=O(['childUserId', 'groupId'], childUserId='uuid', groupId='uuid'),
                   ok=('201', R('Participant'), 'Toegevoegd'), errors=['404', '409']),
    },
    '/api/camps/{campId}/participants/{participantId}': {
        'patch': op(D, 'Kind naar een andere groep verplaatsen', 'Chat- en belrechten schuiven automatisch mee.',
                    roles=['staff'], params=[ID('campId'), ID('participantId')], body=O(['groupId'], groupId='uuid'),
                    ok=('200', R('Participant'), 'OK'), errors=['404']),
        'delete': op(D, 'Kind uit het kamp halen', 'Publiceert `camp.ParticipantRemoved.v1`.', roles=['staff'],
                     params=[ID('campId'), ID('participantId')], ok=('204', None, 'Verwijderd'), errors=['404']),
    },
    '/api/camps/{campId}/staff': {
        'get': op(D, 'Leiding van een kamp', roles=['guardian', 'staff'], params=[ID('campId')],
                  ok=('200', A(R('StaffAssignment')), 'OK'), errors=['404']),
        'post': op(D, 'Leiding toewijzen', 'Zonder `groupId` geldt de toewijzing voor het hele kamp. Publiceert `camp.StaffAssigned.v1`.',
                   roles=['organisation_admin', 'staff'], params=[ID('campId')],
                   body=O(['staffUserId', 'staffRole'], staffUserId='uuid', groupId='uuid', staffRole=R('StaffRole')),
                   ok=('201', R('StaffAssignment'), 'Toegewezen'), errors=['404', '409']),
    },
    '/api/camps/{campId}/staff/{staffAssignmentId}': {
        'delete': op(D, 'Toewijzing intrekken', roles=['organisation_admin', 'staff'],
                     params=[ID('campId'), ID('staffAssignmentId')], ok=('204', None, 'Ingetrokken'), errors=['404']),
    },
    '/api/camps/{campId}/activities': {
        'get': op(AC, 'Programma opvragen', roles=['guardian', 'staff'], params=[ID('campId'), P('groupId', t='uuid')],
                  ok=('200', A(R('Activity')), 'OK'), errors=['404']),
        'post': op(AC, 'Activiteit toevoegen', roles=['staff'], params=[ID('campId')], body=R('ActivityInput'),
                   ok=('201', R('Activity'), 'Toegevoegd'), errors=['404']),
    },
    '/api/camps/{campId}/activities/{activityId}': {
        'put': op(AC, 'Activiteit wijzigen', roles=['staff'], params=[ID('campId'), ID('activityId')], body=R('ActivityInput'),
                  ok=('200', R('Activity'), 'OK'), errors=['404']),
        'delete': op(AC, 'Activiteit verwijderen', roles=['staff'], params=[ID('campId'), ID('activityId')],
                     ok=('204', None, 'Verwijderd'), errors=['404']),
    },
}, {
    'CampStatus': E('planned', 'active', 'ended', 'archived'),
    'StaffRole': E('camp_leader', 'animator'),
    'CampInput': O(['name', 'startDate', 'endDate'], name=S('string', example='Zomerkamp 2027'), description='text',
                   location=S('string', example='Kampplaats De Brand, Ronse'), startDate='date', endDate='date',
                   ageMin=S('int', example=8), ageMax=S('int', example=12)),
    'Camp': {'allOf': [O(['campId', 'status', 'isPublished'], campId='uuid', status=R('CampStatus'), isPublished='bool',
                         publishedAt='datetime', createdAt='datetime'), R('CampInput')]},
    'GroupInput': O(['name'], name=S('string', example='Kabouters'), ageMin='int', ageMax='int'),
    'Group': {'allOf': [O(groupId='uuid', campId='uuid'), R('GroupInput')]},
    'Participant': O(participantId='uuid', childUserId='uuid', displayName=S('string', description='Uit het read-model'),
                     groupId='uuid', joinedAt='datetime', isActive='bool'),
    'StaffAssignment': O(staffAssignmentId='uuid', staffUserId='uuid', displayName='string', groupId='uuid',
                         staffRole=R('StaffRole'), assignedAt='datetime'),
    'ActivityInput': O(['title', 'startsAt', 'endsAt'], title=S('string', example='Dropping'), description='text',
                       groupId=S('uuid', description='Leeg = voor het hele kamp'), startsAt='datetime', endsAt='datetime'),
    'Activity': {'allOf': [O(activityId='uuid'), R('ActivityInput')]},
}))

# ═════════════════════════ 05 Messaging ═════════════════════════
CV = 'Gesprekken'; MS = 'Berichten'; MO = 'Moderatie'
FILES.append(spec('messaging-service.openapi.yaml', 'Messaging Service', """
Chat tussen ouders, kinderen en leiding. Realtime verkeer loopt via de **SignalR-hub** `wss://api.kampconnect.be/hubs/chat`;
deze REST-API dient voor historiek, gesprekken openen en moderatie. Database: `msg_*`.

| Hub-methode (client → server) | Hub-event (server → client) |
|---|---|
| `SendMessage(conversationId, body, attachmentFileIds)` | `MessageReceived(message)` |
| `StartTyping(conversationId)` | `UserTyping(conversationId, userId)` |
| `MarkRead(conversationId, messageId)` | `MessageRead(conversationId, userId, messageId)` |
| | `PresenceChanged(userId, isOnline, lastSeenAt)` |

Wie in welk gesprek mag, volgt uit de `camp.*`-events (eigen deelnemerstabel, geen call naar Camp).
Berichten worden nooit hard verwijderd.
""", [(CV, 'Gesprekken en deelnemers'), (MS, 'Berichten lezen en versturen'), (MO, 'Melden en beoordelen van berichten')], {
    '/api/conversations': {
        'get': op(CV, 'Mijn gesprekken', 'Gesorteerd op laatste bericht (recentste eerst).', roles=['guardian', 'staff'],
                  params=[P('campId', t='uuid')] + PAGE_PARAMS, ok=('200', paged('Conversation'), 'OK')),
        'post': op(CV, '1-op-1-gesprek starten', 'Enkel met iemand binnen hetzelfde kamp. Bestaat het gesprek al, dan komt dat terug.',
                   roles=['guardian', 'staff'], body=O(['userId'], userId='uuid'), ok=('201', R('Conversation'), 'Aangemaakt'),
                   errors=['403']),
    },
    '/api/conversations/{conversationId}': {
        'get': op(CV, 'Gesprek opvragen', roles=['guardian', 'staff'], params=[ID('conversationId')],
                  ok=('200', R('Conversation'), 'OK'), errors=['404']),
    },
    '/api/conversations/{conversationId}/mute': {
        'put': op(CV, 'Gesprek dempen of niet', roles=['guardian', 'staff'], params=[ID('conversationId')],
                  body=O(['isMuted'], isMuted='bool'), ok=('204', None, 'Gewijzigd'), errors=['404']),
    },
    '/api/conversations/{conversationId}/messages': {
        'get': op(MS, 'Berichten ophalen', 'Cursor-paginering: geef `before` mee om oudere berichten te laden.',
                  roles=['guardian', 'staff'],
                  params=[ID('conversationId'), P('before', t='uuid', desc='Bericht-ID'), P('search', desc='Zoeken in de tekst'),
                          P('limit', t='int', default=50, maximum=100)],
                  ok=('200', A(R('Message')), 'OK'), errors=['404']),
        'post': op(MS, 'Bericht versturen (REST-alternatief voor de hub)', 'Publiceert `messaging.MessageSent.v1`.',
                   roles=['guardian', 'staff'], params=[ID('conversationId')], body=R('MessageCreate'),
                   ok=('201', R('Message'), 'Verstuurd'), errors=['404']),
    },
    '/api/conversations/{conversationId}/read': {
        'post': op(MS, 'Gelezen tot en met een bericht', roles=['guardian', 'staff'], params=[ID('conversationId')],
                   body=O(['messageId'], messageId='uuid'), ok=('204', None, 'OK'), errors=['404']),
    },
    '/api/messages/{messageId}': {
        'patch': op(MS, 'Eigen bericht bewerken', roles=['guardian', 'staff'], params=[ID('messageId')],
                    body=O(['body'], body='text'), ok=('200', R('Message'), 'OK'), errors=['404']),
        'delete': op(MS, 'Bericht verwijderen', 'Zacht verwijderd (`is_deleted`). Publiceert `messaging.MessageDeleted.v1`.',
                     roles=['guardian', 'staff'], params=[ID('messageId')], ok=('204', None, 'Verwijderd'), errors=['404']),
    },
    '/api/camps/{campId}/announcements': {
        'post': op(MS, 'Aankondiging naar alle ouders van een kamp', 'Publiceert `messaging.AnnouncementPublished.v1`.',
                   roles=['organisation_admin', 'staff'], params=[ID('campId')],
                   body=O(['title', 'body'], title='string', body='text', groupId=S('uuid', description='Optioneel: enkel één groep')),
                   ok=('201', R('Message'), 'Verstuurd'), errors=['404']),
    },
    '/api/messages/{messageId}/flags': {
        'post': op(MO, 'Bericht melden', 'Publiceert `messaging.MessageFlagged.v1`.', roles=['guardian', 'staff'],
                   params=[ID('messageId')], body=O(['reason'], reason=S('string', maxLength=200)),
                   ok=('201', R('Flag'), 'Gemeld'), errors=['404']),
    },
    '/api/moderation/flags': {
        'get': op(MO, 'Meldingen opvragen', roles=['organisation_admin'], params=[P('status', t=R('FlagStatus'))] + PAGE_PARAMS,
                  ok=('200', paged('Flag'), 'OK')),
    },
    '/api/moderation/flags/{flagId}': {
        'patch': op(MO, 'Melding beoordelen', '`hidden` verbergt het bericht voor iedereen.', roles=['organisation_admin'],
                    params=[ID('flagId')], body=O(['status'], status=R('FlagStatus')), ok=('200', R('Flag'), 'OK'), errors=['404']),
    },
}, {
    'Conversation': O(conversationId='uuid', conversationType=E('direct', 'group', 'announcement'), campId='uuid', groupId='uuid',
                      title='string', participants=A(R('Participant')), lastMessage=R('Message'),
                      unreadCount=S('int', example=2), isMuted='bool', isArchived='bool'),
    'Participant': O(userId='uuid', displayName='string', avatarFileId='uuid', lastSeenAt='datetime'),
    'MessageCreate': O(['body'], body=S('text', maxLength=4000),
                       attachmentFileIds=A(S('uuid', description='Eerst opladen via de Media Service'))),
    'Message': O(messageId='uuid', conversationId='uuid', senderUserId='uuid', body='text',
                 attachments=A(O(fileId='uuid', fileName='string', contentType='string', sizeBytes='long')),
                 sentAt='datetime', editedAt='datetime', isDeleted='bool'),
    'FlagStatus': E('open', 'dismissed', 'hidden'),
    'Flag': O(flagId='uuid', messageId='uuid', reportedByUserId='uuid', reason='string', status=R('FlagStatus'),
              createdAt='datetime', reviewedAt='datetime'),
}))

# ═════════════════════════ 06 Calling ═════════════════════════
CA = 'Oproepen'; TU = 'TURN'
FILES.append(spec('calling-service.openapi.yaml', 'Calling Service', """
Spraak- en video-oproepen via WebRTC. De service vervoert zelf **geen** audio of video: ze regelt het opzetten van de
verbinding (signaling) en deelt tijdelijke TURN-credentials uit. Database: `cal_*`.

Signaling loopt via de **SignalR-hub** `wss://api.kampconnect.be/hubs/calls`:

| Hub-methode (client → server) | Hub-event (server → client) |
|---|---|
| `SendOffer(callId, sdp)` | `OfferReceived(callId, sdp)` |
| `SendAnswer(callId, sdp)` | `AnswerReceived(callId, sdp)` |
| `SendIceCandidate(callId, candidate)` | `IceCandidateReceived(callId, candidate)` |
| | `CallStateChanged(callId, status)` |

Belrechten worden vooraf opgebouwd uit de `camp.*`-events. Gesprekken worden niet opgenomen; enkel metadata.
""", [(CA, 'Levenscyclus en historiek van oproepen'), (TU, 'Tijdelijke credentials voor de TURN-server')], {
    '/api/calls': {
        'get': op(CA, 'Oproepgeschiedenis', roles=['guardian', 'staff'], params=PAGE_PARAMS, ok=('200', paged('Call'), 'OK')),
        'post': op(CA, 'Oproep starten',
                   'Controleert de lokale belrechten. Het antwoord bevat meteen TURN-credentials. Publiceert `calling.CallInitiated.v1` (of `calling.EmergencyCallPlaced.v1`).',
                   roles=['guardian', 'staff'], body=R('CallCreate'), ok=('201', R('CallStarted'), 'Oproep gestart'), errors=['403']),
    },
    '/api/calls/{callId}': {
        'get': op(CA, 'Oproep opvragen', roles=['guardian', 'staff'], params=[ID('callId')], ok=('200', R('Call'), 'OK'), errors=['404']),
    },
    '/api/calls/{callId}/answer': {
        'post': op(CA, 'Opnemen', 'Publiceert `calling.CallAnswered.v1`.', roles=['guardian', 'staff'], params=[ID('callId')],
                   ok=('200', R('CallStarted'), 'OK'), errors=['404', '409']),
    },
    '/api/calls/{callId}/decline': {
        'post': op(CA, 'Weigeren', roles=['guardian', 'staff'], params=[ID('callId')], ok=('204', None, 'OK'), errors=['404', '409']),
    },
    '/api/calls/{callId}/end': {
        'post': op(CA, 'Ophangen', 'Publiceert `calling.CallEnded.v1`.', roles=['guardian', 'staff'], params=[ID('callId')],
                   body=O(endReason=E('hangup', 'network_error', 'timeout')), ok=('200', R('Call'), 'OK'), errors=['404']),
    },
    '/api/calls/{callId}/quality-samples': {
        'post': op(CA, 'Kwaliteitsmeting doorgeven', 'Wordt periodiek door de app verstuurd tijdens een gesprek.',
                   roles=['guardian', 'staff'], params=[ID('callId')],
                   body=O(['roundTripMs', 'jitterMs', 'packetLossPct'], roundTripMs='int', jitterMs='int',
                          packetLossPct=S('number', example=1.5)),
                   ok=('204', None, 'Opgeslagen'), errors=['404']),
    },
    '/api/turn-credentials': {
        'post': op(TU, 'TURN-credentials opvragen', 'Enkel voor een lopende oproep. Enkele uren geldig.',
                   roles=['guardian', 'staff'], body=O(['callId'], callId='uuid'), ok=('201', R('TurnCredentials'), 'OK'),
                   errors=['404']),
    },
}, {
    'CallStatus': E('initiated', 'ringing', 'answered', 'declined', 'missed', 'ended', 'forwarded'),
    'CallCreate': O(['calleeUserId', 'callType'], calleeUserId='uuid', campId='uuid', callType=E('audio', 'video'),
                    isEmergency=S('bool', default=False, description='Verhoogde prioriteit, wordt doorgeschakeld naar het bureau')),
    'Call': O(callId='uuid', callerUserId='uuid', calleeUserId='uuid', campId='uuid', callType=E('audio', 'video'),
              status=R('CallStatus'), isEmergency='bool', connectionType=E('direct', 'relay'),
              initiatedAt='datetime', answeredAt='datetime', endedAt='datetime', durationSeconds='int', endReason='string'),
    'CallStarted': O(call=R('Call'), iceServers=A(R('IceServer'))),
    'TurnCredentials': O(iceServers=A(R('IceServer')), expiresAt='datetime'),
    'IceServer': O(['urls'], urls=A(S('string', example='turns:turn.kampconnect.be:443?transport=tcp')),
                   username=S('string', example='1790180000:58'), credential='string'),
}))

# ═════════════════════════ 07 Billing ═════════════════════════
B = 'Abonnement'; IV = 'Facturen'
FILES.append(spec('billing-service.openapi.yaml', 'Billing Service', """
Wat een organisatie aan KampConnect verschuldigd is: formules, abonnement, verbruik en facturen.
Billing int zelf geen geld (dat doet Payment). Bedragen staan in **centen** met een valuta. Database: `bil_*`.
""", [(B, 'Formules, abonnement en verbruik'), (IV, 'Facturen en creditnota\'s')], {
    '/api/billing/plans': {
        'get': op(B, 'Beschikbare formules', public=True, ok=('200', A(R('Plan')), 'OK')),
    },
    '/api/billing/subscription': {
        'get': op(B, 'Huidig abonnement', roles=['organisation_admin'], ok=('200', R('Subscription'), 'OK')),
    },
    '/api/billing/subscription/change': {
        'post': op(B, 'Van formule wisselen', 'Gaat in bij de volgende periode.', roles=['organisation_admin'],
                   body=O(['planCode'], planCode=S('string', example='standard')), ok=('202', R('Subscription'), 'Ingepland'),
                   errors=['409']),
    },
    '/api/billing/usage': {
        'get': op(B, 'Verbruik in de huidige periode', roles=['organisation_admin'], ok=('200', A(R('Usage')), 'OK')),
    },
    '/api/billing/invoices': {
        'get': op(IV, 'Facturen opvragen', roles=['organisation_admin'], params=[P('status', t=R('InvoiceStatus'))] + PAGE_PARAMS,
                  ok=('200', paged('Invoice'), 'OK')),
    },
    '/api/billing/invoices/{invoiceId}': {
        'get': op(IV, 'Factuur met lijnen', roles=['organisation_admin'], params=[ID('invoiceId')],
                  ok=('200', R('InvoiceDetail'), 'OK'), errors=['404']),
    },
    '/api/billing/invoices/{invoiceId}/pdf': {
        'get': {**op(IV, 'Factuur als PDF', roles=['organisation_admin'], params=[ID('invoiceId')], errors=['404']),
                'responses': {'200': {'description': 'PDF-bestand', 'content': {'application/pdf': {'schema': {'type': 'string', 'format': 'binary'}}}},
                              '401': {'$ref': '#/components/responses/Unauthorized'}, '404': {'$ref': '#/components/responses/NotFound'}}},
    },
    '/api/billing/invoices/{invoiceId}/credit-notes': {
        'post': op(IV, 'Creditnota maken', 'Een verstuurde factuur wijzigt nooit; correcties gebeuren met een creditnota.',
                   roles=['platform_admin'], params=[ID('invoiceId')],
                   body=O(['reason', 'amountCents'], reason='string', amountCents='long'),
                   ok=('201', R('Invoice'), 'Aangemaakt'), errors=['404']),
    },
}, {
    'Plan': O(code=S('string', example='standard'), name=S('string', example='Standaard'),
              billingInterval=E('monthly', 'yearly'), priceCents=S('long', example=4900), currency=S('string', example='EUR'),
              limits=A(O(metric=R('Metric'), includedQuantity='long', overageUnitPriceCents='long'))),
    'Metric': E('camps', 'users', 'storage_bytes'),
    'Subscription': O(subscriptionId='uuid', plan=R('Plan'), status=E('trialing', 'active', 'past_due', 'cancelled'),
                      currentPeriodStart='datetime', currentPeriodEnd='datetime', trialEndsAt='datetime', cancelAt='datetime'),
    'Usage': O(metric=R('Metric'), quantity='long', includedQuantity='long', periodStart='datetime', periodEnd='datetime'),
    'InvoiceStatus': E('draft', 'open', 'paid', 'overdue', 'void'),
    'Invoice': O(invoiceId='uuid', invoiceNumber=S('string', example='2027-00042'), documentType=E('invoice', 'credit_note'),
                 status=R('InvoiceStatus'), currency=S('string', example='EUR'), subtotalCents='long', vatCents='long',
                 totalCents=S('long', example=5929), issuedAt='datetime', dueAt='datetime', paidAt='datetime'),
    'InvoiceDetail': {'allOf': [R('Invoice'), O(lines=A(O(description='string', quantity='number', unitPriceCents='long',
                                                            vatRate=S('number', example=21), lineTotalCents='long')))]},
}))

# ═════════════════════════ 08 Payment ═════════════════════════
PY = 'Betalingen'; WH = 'Webhooks'
FILES.append(spec('payment-service.openapi.yaml', 'Payment Service', """
Int het geld voor openstaande facturen via een externe betaalprovider (bv. Mollie). KampConnect raakt nooit
kaartgegevens aan: de betaler wordt doorgestuurd naar de pagina van de provider.
**De webhook is de waarheid**, niet de terugkeerpagina. Database: `pay_*`.
""", [(PY, 'Betalingen starten en opvolgen'), (WH, 'Berichten van de betaalprovider')], {
    '/api/payments': {
        'get': op(PY, 'Betalingen opvragen', roles=['organisation_admin'], params=PAGE_PARAMS, ok=('200', paged('Payment'), 'OK')),
        'post': op(PY, 'Betaling starten voor een factuur', 'Stuur de gebruiker daarna naar `checkoutUrl`.',
                   roles=['organisation_admin'],
                   params=[P('Idempotency-Key', 'header', required=True, desc='Uniek per poging; voorkomt dubbele betalingen')],
                   body=O(['invoiceId'], invoiceId='uuid', returnUrl='uri'), ok=('201', R('Payment'), 'Aangemaakt'),
                   errors=['404', '409']),
    },
    '/api/payments/{paymentId}': {
        'get': op(PY, 'Betaling opvragen', roles=['organisation_admin'], params=[ID('paymentId')],
                  ok=('200', R('Payment'), 'OK'), errors=['404']),
    },
    '/api/payments/{paymentId}/refunds': {
        'post': op(PY, 'Terugbetaling starten', roles=['platform_admin'], params=[ID('paymentId')],
                   body=O(['amountCents', 'reason'], amountCents='long', reason='string'),
                   ok=('201', R('Refund'), 'Aangevraagd'), errors=['404', '409']),
    },
    '/api/webhooks/{providerCode}': {
        'post': op(WH, 'Webhook van de betaalprovider',
                   'De handtekening wordt gecontroleerd en elk bericht wordt maar één keer verwerkt. Bij een geslaagde betaling volgt `payment.PaymentSucceeded.v1`.',
                   public=True, params=[P('providerCode', 'path', example='mollie')],
                   body={'type': 'object', 'additionalProperties': True, 'description': 'Formaat van de provider'},
                   ok=('200', None, 'Ontvangen'), errors=['400']),
    },
}, {
    'PaymentStatus': E('pending', 'authorized', 'succeeded', 'failed', 'expired', 'refunded', 'charged_back'),
    'Payment': O(paymentId='uuid', invoiceId='uuid', status=R('PaymentStatus'), amountCents=S('long', example=5929),
                 currency=S('string', example='EUR'), paymentMethod=S('string', example='bancontact'),
                 checkoutUrl=S('uri', description='Enkel zolang de betaling `pending` is'),
                 createdAt='datetime', succeededAt='datetime', failureMessage='string'),
    'Refund': O(refundId='uuid', paymentId='uuid', status=E('pending', 'succeeded', 'failed'), amountCents='long',
                reason='string', requestedAt='datetime'),
}))

# ═════════════════════════ 09 Notification ═════════════════════════
NT = 'Toestellen'; NP = 'Voorkeuren'; NH = 'Meldingen'
FILES.append(spec('notification-service.openapi.yaml', 'Notification Service', """
Bereikt gebruikers buiten de app (push en e-mail). De meldingen zelf ontstaan **uitsluitend uit events**; deze kleine
REST-API dient enkel om toesteltokens te registreren en voorkeuren in te stellen. Database: `ntf_*`.
""", [(NT, 'Pushtokens van de mobiele app'), (NP, 'Welke meldingen, via welk kanaal, en stille uren'),
      (NH, 'Overzicht van ontvangen meldingen')], {
    '/api/notifications/device-tokens': {
        'post': op(NT, 'Toestel registreren', 'Aanroepen na het inloggen en telkens FCM/APNs een nieuw token geeft.',
                   roles=['guardian', 'staff'], body=R('DeviceTokenCreate'), ok=('201', R('DeviceToken'), 'Geregistreerd')),
    },
    '/api/notifications/device-tokens/{deviceTokenId}': {
        'delete': op(NT, 'Toestel afmelden (bij uitloggen)', roles=['guardian', 'staff'], params=[ID('deviceTokenId')],
                     ok=('204', None, 'Afgemeld'), errors=['404']),
    },
    '/api/notifications/preferences': {
        'get': op(NP, 'Mijn meldingsvoorkeuren', roles=['guardian', 'staff'], ok=('200', A(R('Preference')), 'OK')),
        'put': op(NP, 'Voorkeuren opslaan', 'Verplichte meldingen (bv. noodoproepen) kunnen niet uitgezet worden.',
                  roles=['guardian', 'staff'], body=A(R('Preference')), ok=('200', A(R('Preference')), 'OK')),
    },
    '/api/notifications/quiet-hours': {
        'get': op(NP, 'Stille uren opvragen', roles=['guardian', 'staff'], ok=('200', R('QuietHours'), 'OK')),
        'put': op(NP, 'Stille uren instellen', roles=['guardian', 'staff'], body=R('QuietHours'), ok=('200', R('QuietHours'), 'OK')),
    },
    '/api/notifications': {
        'get': op(NH, 'Mijn meldingen', roles=['guardian', 'staff'], params=PAGE_PARAMS, ok=('200', paged('Notification'), 'OK')),
    },
}, {
    'DeviceTokenCreate': O(['platform', 'token'], platform=E('android', 'ios', 'web'), token='string',
                           appVersion=S('string', example='1.0.3'), language=S('string', example='nl')),
    'DeviceToken': O(deviceTokenId='uuid', platform=E('android', 'ios', 'web'), registeredAt='datetime'),
    'Preference': O(['notificationTypeCode', 'channel', 'isEnabled'], notificationTypeCode=S('string', example='message_received'),
                    channel=E('push', 'email'), isEnabled='bool',
                    isMandatory=S('bool', readOnly=True)),
    'QuietHours': O(['isEnabled'], isEnabled='bool', startTime=S('time', example='22:00'), endTime=S('time', example='07:00'),
                    timezone=S('string', example='Europe/Brussels')),
    'Notification': O(notificationId='uuid', notificationTypeCode='string', title='string', body='string',
                      createdAt='datetime', sentAt='datetime'),
}))

# ═════════════════════════ 10 Compliance ═════════════════════════
CO = 'Toestemmingen'; RQ = 'GDPR-verzoeken'; AU = 'Audit & incidenten'
FILES.append(spec('compliance-service.openapi.yaml', 'Compliance Service', """
Verantwoording over persoonsgegevens: auditlog (append-only), toestemmingen met versie, GDPR-verzoeken
(inzage en verwijdering) en incidenten. Verwijderen = anonimiseren, gecoördineerd via events. Database: `cpl_*`.
""", [(CO, 'Privacybeleid en toestemmingen'), (RQ, 'Inzage- en verwijderverzoeken'), (AU, 'Auditlog en incidenten (bureau)')], {
    '/api/compliance/policies': {
        'get': op(CO, 'Geldende beleidsdocumenten', public=True, params=[P('language', example='nl')],
                  ok=('200', A(R('PolicyDocument')), 'OK')),
    },
    '/api/compliance/consents': {
        'get': op(CO, 'Toestemmingen opvragen', 'Een ouder ziet die van zichzelf en zijn kinderen.', roles=['guardian', 'staff'],
                  params=[P('userId', t='uuid')], ok=('200', A(R('Consent')), 'OK')),
        'post': op(CO, 'Toestemming geven of intrekken', 'Bv. foto\'s publiceren of alleen naar huis mogen. Wordt versiegewijs bewaard.',
                   roles=['guardian'], body=R('ConsentCreate'), ok=('201', R('Consent'), 'Geregistreerd')),
    },
    '/api/compliance/requests': {
        'get': op(RQ, 'Verzoeken opvragen', 'Een gebruiker ziet de eigen verzoeken, een beheerder die van de organisatie.',
                  roles=['guardian', 'organisation_admin'], params=PAGE_PARAMS, ok=('200', paged('DataSubjectRequest'), 'OK')),
        'post': op(RQ, 'Inzage of verwijdering aanvragen', 'Bij verwijdering volgt `compliance.ErasureRequested.v1`.',
                   roles=['guardian', 'staff'], body=O(['requestType'], requestType=E('access', 'erasure'),
                                                        subjectUserId=S('uuid', description='Leeg = mezelf; anders een eigen kind')),
                   ok=('202', R('DataSubjectRequest'), 'Ontvangen')),
    },
    '/api/compliance/requests/{requestId}': {
        'get': op(RQ, 'Status van een verzoek', 'Toont per service of die al bevestigd heeft.',
                  roles=['guardian', 'organisation_admin'], params=[ID('requestId')], ok=('200', R('DataSubjectRequest'), 'OK'),
                  errors=['404']),
    },
    '/api/compliance/requests/{requestId}/export': {
        'get': op(RQ, 'Downloadlink van het gegevensuittreksel', 'Tijdelijke link via de Media Service.', roles=['guardian'],
                  params=[ID('requestId')], ok=('200', O(downloadUrl='uri', expiresAt='datetime'), 'OK'), errors=['404', '409']),
    },
    '/api/compliance/audit-entries': {
        'get': op(AU, 'Auditlog doorzoeken', 'Alleen-lezen: het auditlog kan niet gewijzigd worden.', roles=['organisation_admin'],
                  params=[P('subjectUserId', t='uuid'), P('eventName', example='auth.PasswordChanged.v1'),
                          P('from', t='datetime'), P('to', t='datetime')] + PAGE_PARAMS,
                  ok=('200', paged('AuditEntry'), 'OK')),
    },
    '/api/compliance/incidents': {
        'get': op(AU, 'Incidenten opvragen', roles=['organisation_admin'], params=[P('status', t=R('IncidentStatus'))] + PAGE_PARAMS,
                  ok=('200', paged('Incident'), 'OK')),
        'post': op(AU, 'Incident melden', roles=['staff', 'organisation_admin'], body=R('IncidentCreate'),
                   ok=('201', R('Incident'), 'Gemeld')),
    },
    '/api/compliance/incidents/{incidentId}': {
        'patch': op(AU, 'Incident opvolgen of afsluiten', roles=['organisation_admin'], params=[ID('incidentId')],
                    body=O(status=R('IncidentStatus'), resolution='text'), ok=('200', R('Incident'), 'OK'), errors=['404']),
    },
}, {
    'PolicyDocument': O(policyDocumentId='uuid', policyType=E('privacy_policy', 'terms', 'photo_consent', 'go_home_alone'),
                        version=S('int', example=3), language=S('string', example='nl'), url='uri', publishedAt='datetime'),
    'ConsentCreate': O(['policyDocumentId', 'action'], policyDocumentId='uuid', action=E('given', 'withdrawn'),
                       userId=S('uuid', description='Leeg = mezelf; anders een eigen kind')),
    'Consent': O(consentRecordId='uuid', userId='uuid', policyDocumentId='uuid', policyType='string', version='int',
                 action=E('given', 'withdrawn'), givenByUserId='uuid', occurredAt='datetime'),
    'DataSubjectRequest': O(requestId='uuid', requestType=E('access', 'erasure'),
                            status=E('received', 'in_progress', 'completed', 'rejected'), subjectUserId='uuid',
                            receivedAt='datetime', dueAt=S('datetime', description='Wettelijke termijn: 1 maand'),
                            completedAt='datetime',
                            tasks=A(O(serviceName=S('string', example='messaging'), status=E('pending', 'done', 'failed'),
                                      completedAt='datetime'))),
    'AuditEntry': O(auditEntryId='uuid', eventName='string', actorUserId='uuid', subjectUserId='uuid', resourceType='string',
                    resourceId='uuid', occurredAt='datetime'),
    'IncidentStatus': E('open', 'investigating', 'resolved'),
    'IncidentCreate': O(['incidentType', 'severity', 'description'], incidentType=E('safety', 'data_breach', 'misconduct', 'other'),
                        severity=S('int', minimum=1, maximum=4), subjectUserId='uuid', description='text'),
    'Incident': {'allOf': [O(incidentId='uuid', status=R('IncidentStatus'), openedAt='datetime', resolvedAt='datetime',
                             resolution='text'), R('IncidentCreate')]},
}))

# ═════════════════════════ 11 Media ═════════════════════════
F = 'Bestanden'
FILES.append(spec('media-service.openapi.yaml', 'Media Service', """
Bewaart en levert bestanden: profielfoto's, bijlagen en kampfoto's. Bestanden staan in privé-objectopslag;
downloaden kan enkel via een **tijdelijke, ondertekende link**. Het type wordt op de inhoud gecontroleerd,
niet op de extensie. Eerst opladen, daarna koppelt de andere service het bestand. Database: `med_*`.
""", [(F, 'Opladen, opvragen en downloaden')], {
    '/api/files': {
        'post': op(F, 'Bestand opladen', 'Toegelaten: JPEG, PNG, HEIC, PDF — max. 20 MB. Niet-gekoppelde bestanden worden na 24 u opgeruimd.',
                   roles=['guardian', 'staff'], content_type='multipart/form-data',
                   body=O(['file', 'purpose'], file=S('string', format='binary'), purpose=R('FilePurpose'), campId='uuid'),
                   ok=('201', R('File'), 'Opgeladen'), errors=['409']),
    },
    '/api/files/{fileId}': {
        'get': op(F, 'Metadata opvragen', roles=['guardian', 'staff'], params=[ID('fileId')], ok=('200', R('File'), 'OK'), errors=['404']),
        'delete': op(F, 'Bestand verwijderen', roles=['guardian', 'staff'], params=[ID('fileId')],
                     ok=('204', None, 'Verwijderd'), errors=['404']),
    },
    '/api/files/{fileId}/download-url': {
        'get': op(F, 'Tijdelijke downloadlink', 'De link is enkele minuten geldig.', roles=['guardian', 'staff'],
                  params=[ID('fileId'), P('variant', t=E('original', 'thumbnail', 'medium'), default='original')],
                  ok=('200', O(url='uri', expiresAt='datetime'), 'OK'), errors=['404']),
    },
    '/api/files/storage-usage': {
        'get': op(F, 'Opslagverbruik van de organisatie', roles=['organisation_admin'],
                  ok=('200', O(totalBytes='long', fileCount='int', limitBytes='long'), 'OK')),
    },
}, {
    'FilePurpose': E('avatar', 'message_attachment', 'camp_photo', 'data_export'),
    'File': O(fileId='uuid', purpose=R('FilePurpose'), status=E('uploaded', 'scanning', 'available', 'rejected', 'deleted'),
              originalFileName='string', contentType=S('string', example='image/jpeg'), sizeBytes='long',
              width='int', height='int', uploadedByUserId='uuid', uploadedAt='datetime'),
}))

print('\n'.join(service_path(f) for f in FILES))
