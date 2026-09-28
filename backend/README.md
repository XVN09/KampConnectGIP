# KampConnect – backend

Microservices in C# .NET 10, samen in één solution: `KampConnect.slnx`.
Het overzicht van de hele repository staat in de [README in de root](../README.md).

## Structuur

De mappen op schijf en de solution folders in `KampConnect.slnx` zijn identiek.

| Map | Inhoud |
|---|---|
| `000-Shared` | Gedeelde contracten (events), basisconfiguratie van services en helpers (logging) |
| `100-Gateway` | API-gateway, het enige toegangspunt voor de app |
| `200-Platform` | Identity, Tenant, User, Compliance |
| `300-Camp-Communication` | Camp, Messaging, Calling, Media, Notification |
| `400-Payments` | Billing, Payment |
| `800 - Hosting` | Aspire AppHost, die alle services lokaal start |
| `900-Tests` | Eén testproject per service |
| `scripts` | Scripts om services aan te maken en de structuur te controleren |

Elke service (bv. `310-Camp`) heeft drie lagen:

```
310-Camp/
├─ 311-Data/      Entity, Context, Interface, Repository
├─ 312-Business/  Contracts, Logic
└─ 313-Service/   Service (ASP.NET Core Web API)
```

Een service kan daarnaast console-apps hebben in een map `NNN-Console Apps`, bv. `210-Identity/214-Console Apps`.

## Regels

- Voeg projecten nooit toe via "Add New Project" in Visual Studio en versleep niets in de Solution Explorer.
- Een nieuwe service maak je aan met (vanuit de root van de repository):
  ```powershell
  .\backend\scripts\New-Service.ps1 -Name Camp -Group 300-Camp-Communication -Number 310
  ```
- Controleer de structuur en de afhankelijkheidsregels met:
  ```powershell
  .\backend\scripts\Test-Structure.ps1
  ```
- Packageversies staan enkel in `Directory.Packages.props`, gedeelde projectinstellingen in `Directory.Build.props`.
  De versie van de .NET SDK staat in `global.json` in de root van de repository.
- Welke laag naar welke mag verwijzen, staat in [ADR 0001](../docs/adr/0001-projectstructuur.md)
  en [ADR 0003](../docs/adr/0003-regels-helpers-hosting.md).

Geeft PowerShell een fout over de execution policy, voer dan eenmalig uit:
`Set-ExecutionPolicy -Scope CurrentUser RemoteSigned`
