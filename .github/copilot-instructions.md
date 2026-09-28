# Copilot Instructions

## Project Guidelines
- De gebruiker heeft package source mapping geconfigureerd in de globale NuGet.Config (%APPDATA%\NuGet\NuGet.Config), oorspronkelijk enkel met patroon 'KampConnect.Config'. Alle overige packages moeten via nuget.org toegankelijk zijn met een wildcard '*' patroon toegevoegd aan de nuget.org bron.
- Voor het KampConnectGIP-project: gevoelige waarden zoals wachtwoorden voor de testgebruiker in KampConnect.Identity.ClientRegistration mogen niet in appsettings.json/appsettings.Development.json staan, maar moeten via dotnet user-secrets worden ingesteld (Registration:TestUser:Password).
