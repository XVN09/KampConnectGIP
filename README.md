# KampConnect

Communicatie-app voor jeugdkampen: een Flutter-app en webfrontends met een microservices-backend in C# .NET.
Alles zit in één repository; waarom staat in [ADR 0002](docs/adr/0002-monorepo.md).

## Structuur

| Map | Inhoud |
|---|---|
| `backend/` | .NET-solution met alle microservices, de gateway, de AppHost en de tests ([README](backend/README.md)) |
| `apps/` | Frontends: Flutter-app, webdashboard en ouderportaal |
| `packages/` | Gedeelde Dart-code voor de apps (API-client, modellen, thema) |
| `api/` | OpenAPI-specificaties: het contract tussen backend en frontends |
| `infra/` | Configuratie om KampConnect op een server te draaien |
| `docs/` | Architectuurbeslissingen (`docs/adr`) en andere documentatie |

In de root staat enkel wat voor de hele repository geldt: `global.json` (versie van de .NET SDK),
`.editorconfig`, `.gitattributes` en een algemene `.gitignore`. Een deelmap kan daarnaast een eigen
`.gitignore` hebben, bv. `backend/.gitignore` voor Visual Studio.

## Aan de slag

Open `backend/KampConnect.slnx` in Visual Studio, of werk vanuit de root van de repository:

```powershell
dotnet build backend\KampConnect.slnx
dotnet test backend\KampConnect.slnx
.\backend\scripts\Test-Structure.ps1
```

Alle services samen start je lokaal met de Aspire AppHost (`backend/800 - Hosting/KampConnect.Hosting.AppHost`).
