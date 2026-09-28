# Logging

Deze module bevat de gedeelde logging-infrastructuur van KampConnect, opgebouwd rond `Microsoft.Extensions.Logging`.

> **Herkomst:** De logger is overgenomen van het Activak-project en aangepast/hernoemd naar de KampConnect-naamgeving (bv. `KampConnectLogProvider`, `KampConnectLogger`, `KampConnectLogFileWriter`).

## Projecten

- **KampConnect.Shared.Logging.Config** - Bevat de configuratiemodellen (o.a. `LoggingConfigurationData`, `CategoryLoggingData`) die gebruikt worden om de logger op te zetten vanuit configuratie (bv. `appsettings.json`).
- **KampConnect.Shared.Logging.Core** - Bevat de effectieve implementatie van de logger:
  - `KampConnectLogProvider` - `ILoggerProvider`-implementatie die loggers aanmaakt per categorie en logbestanden beheert.
  - `KampConnectLogger` - `ILogger`-implementatie die logberichten opbouwt en wegschrijft.
  - `KampConnectLogFileWriter` - Schrijft logberichten weg naar bestand, inclusief rotatie en archivering.
  - `KampConnectLoggerEx` - Extension methods voor registratie via dependency injection (`UseKampConnectLogger`).

## Gebruik

Registreer de logger via de `IServiceCollection` extension:

```csharp
services.UseKampConnectLogger(sc => /* haal LoggingConfigurationData op, bv. uit configuratie */);
```
