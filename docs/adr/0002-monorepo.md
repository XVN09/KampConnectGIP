# ADR 0002 – Monorepo: backend en frontends in één repository

- **Status:** aanvaard
- **Datum:** 2026-09-28
- **Vult aan:** [ADR 0001](0001-projectstructuur.md)

## Context

Naast de backend komen er een Flutter-app, een webdashboard en een ouderportaal. Die gebruiken dezelfde API,
dus een wijziging aan de API raakt meestal de specificatie, een service, soms de gateway en een of meer frontends.
KampConnect wordt door één ontwikkelaar gebouwd en wordt voor de GIP als één geheel beoordeeld.

## Beslissing

- Eén repository voor backend, frontends, API-specificaties en documentatie.
- De volledige .NET-solution verhuist naar `backend/`: de genummerde groepen, `KampConnect.slnx`,
  `Directory.Build.props`, `Directory.Packages.props`, `EntityTemplate.cs`, `scripts/` en de `.gitignore`
  van Visual Studio. De opbouw binnen de solution uit ADR 0001 verandert niet.
- Nieuwe mappen in de root: `apps/`, `packages/`, `api/` en `infra/`.
- In de root blijft wat voor de hele repository geldt: `global.json`, `.editorconfig`, `.gitattributes`,
  een algemene `.gitignore`, `docs/` en `.github/`.
- De scripts bepalen de backend-map vanuit hun eigen locatie en werken dus vanuit elke map.

## Gevolgen

- Een wijziging aan API, backend en frontend kan in één commit.
- 000-Shared blijft een gewone projectreferentie; een NuGet-package voor gedeelde code is niet nodig.
- De services blijven onafhankelijk: elke service heeft een eigen database, services praten enkel via HTTP
  en berichten met elkaar, er zijn geen projectreferenties tussen services (gecontroleerd door
  `Test-Structure.ps1`) en elke service wordt apart gebouwd en uitgerold. Een repository is een manier om
  code te organiseren, geen architectuurkeuze.
- Een CI-pipeline bouwt later per onderdeel met path-filters, zodat een wijziging aan een app niet alle
  services opnieuw bouwt.
- De `.gitignore` van Visual Studio geldt enkel nog in `backend/`. Regels zoals `[Dd]ebug/`, `[Bb]in/` en
  `**/[Pp]ackages/*` zouden anders bestanden van de Flutter-app en de map `packages/` negeren.
