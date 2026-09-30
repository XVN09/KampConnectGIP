# Copilot Instructions

## Project Guidelines
- De gebruiker heeft package source mapping geconfigureerd in de globale NuGet.Config (%APPDATA%\NuGet\NuGet.Config), oorspronkelijk enkel met patroon 'KampConnect.Config'. Alle overige packages moeten via nuget.org toegankelijk zijn met een wildcard '*' patroon toegevoegd aan de nuget.org bron.
- Voor het KampConnectGIP-project: gevoelige waarden zoals wachtwoorden voor de testgebruiker in KampConnect.Identity.ClientRegistration mogen niet in appsettings.json/appsettings.Development.json staan, maar moeten via dotnet user-secrets worden ingesteld (Registration:TestUser:Password).
- Voor het KampConnectGIP-project wordt PostgreSQL (via Npgsql.EntityFrameworkCore.PostgreSQL) gebruikt als databaseprovider voor de Tenant-service, niet SQL Server.
- Voor het KampConnectGIP-project moet het EF Core Design-package (Microsoft.EntityFrameworkCore.Design) ook aanwezig zijn in het startup-project (bv. KampConnect.Tenant.Service) naast het data-context project, anders faalt 'dotnet ef migrations add' met de foutmelding dat het startup project Microsoft.EntityFrameworkCore.Design niet referenceert.
- Voor het KampConnectGIP-project: schrijf XML-documentatiecommentaar (/// summary enz.) in het Nederlands.
