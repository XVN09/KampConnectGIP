# KampConnect

Communicatie-app voor jeugdkampen: Flutter-app met een microservices-backend in C# .NET.

## Structuur

De mappen op schijf en de solution folders in `KampConnect.slnx` zijn identiek.

| Map | Inhoud |
|---|---|
| `000-Shared` | Gedeelde contracten (events) en basisconfiguratie van services |
| `100-Gateway` | API-gateway, het enige toegangspunt voor de app |
| `200-Platform` | Identity, Tenant, User, Compliance |
| `300-Kamp-Communicatie` | Camp, Messaging, Calling, Media, Notification |
| `400-Betalingen` | Billing, Payment |
| `900-Tests` | Eén testproject per service |
| `scripts` | Scripts om services aan te maken en de structuur te controleren |
| `docs/adr` | Vastgelegde architectuurbeslissingen |

Elke service (bv. `310-Camp`) heeft drie lagen:

```
310-Camp/
├─ 311-Data/      Entity, Context, Interface, Repository
├─ 312-Business/  Contracts, Logic
└─ 313-Service/   Service (ASP.NET Core Web API)
```

## Regels

- Voeg projecten nooit toe via "Add New Project" in Visual Studio en versleep niets in de Solution Explorer.
- Een nieuwe service maak je aan met:
  ```powershell
  .\scripts\New-Service.ps1 -Name Camp -Group 300-Kamp-Communicatie -Number 310
  ```
- Controleer de structuur en de afhankelijkheidsregels met:
  ```powershell
  .\scripts\Test-Structure.ps1
  ```
- Packageversies staan enkel in `Directory.Packages.props`, gedeelde projectinstellingen in `Directory.Build.props`.

Geeft PowerShell een fout over de execution policy, voer dan eenmalig uit:
`Set-ExecutionPolicy -Scope CurrentUser RemoteSigned`
