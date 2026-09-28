# ADR 0003 – Afhankelijkheidsregels voor helpers, console-apps en hosting

- **Status:** aanvaard
- **Datum:** 2026-09-28
- **Vult aan:** [ADR 0001](0001-projectstructuur.md)

## Context

Na ADR 0001 kwamen er projecten bij die niet in het vaste patroon van een service passen: de gedeelde logging
(`000-Shared/020-Helpers/021-Logging`), de console-app `KampConnect.Identity.ClientRegistration` en de
Aspire AppHost. `Test-Structure.ps1` kende ze niet en faalde. Daarnaast waren de solution folders van twee
groepen in Visual Studio hernoemd naar `300-Camp-Communication` en `400-Payments`, terwijl de mappen op schijf
`300-Kamp-Communicatie` en `400-Betalingen` heten.

## Beslissing

- `Shared.Logging.Config` bevat enkel configuratie en hulpfuncties (zoals `DumpObject()`) en mag vanuit
  elk project gebruikt worden.
- `Shared.Logging.Core`, de logger zelf, mag enkel vanuit de Service-laag en vanuit console-apps gebruikt worden.
- Console-apps staan in een map `NNN-Console Apps` binnen hun service en mogen de Data- en Business-lagen
  van die service gebruiken.
- De AppHost start alle services. Het is het enige project dat naar meerdere services mag verwijzen,
  en dan enkel naar hun Service-project en naar de gateway.
- De map van de AppHost heet zoals het project: `800 - Hosting/KampConnect.Hosting.AppHost`.
- Alle groepen krijgen Engelse namen, net als de services en projecten: `300-Kamp-Communicatie` wordt
  `300-Camp-Communication` en `400-Betalingen` wordt `400-Payments`, op schijf én in de solution.

## Gevolgen

- `Test-Structure.ps1` controleert ook deze projecten en slaagt opnieuw.
- Mappen op schijf en solution folders blijven gelijk (ADR 0001); een hernoeming gebeurt altijd op beide plaatsen.
