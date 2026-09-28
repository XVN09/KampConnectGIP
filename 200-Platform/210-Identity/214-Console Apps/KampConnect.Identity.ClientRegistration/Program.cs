using KampConnect.Identity.ClientRegistration;
using KampConnect.Identity.ClientRegistration.ClassTypes;
using KampConnect.Identity.Data.Context.Implentation;
using KampConnect.Shared.Logging.Config.ClassTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(AddConfiguration)
    .ConfigureServices(AddServices)
    .Build();

using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;
var logger = services.GetRequiredService<ILogger<Program>>();

try
{
    var app = services.GetRequiredService<ClientRegistrationMainService>();
    await app.RunAsync();
    return 0;
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Registratie mislukt");
    return 1;
}

void AddConfiguration(HostBuilderContext context, IConfigurationBuilder config)
{
    var environment = context.HostingEnvironment.EnvironmentName;

    config.Sources.Clear();
    config.SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
        .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
        .AddUserSecrets<Program>(optional: true)
        .AddEnvironmentVariables()
        .AddCommandLine(args);
}

void AddServices(HostBuilderContext context, IServiceCollection serviceCollection)
{
    var configuration = context.Configuration;

    // Logging
    var loggingConfiguration = configuration.GetSection("Logging").Get<LoggingConfigurationData>()
        ?? throw new KeyNotFoundException("Section <<Logging>> not found in application settings");
    serviceCollection.UseKampConnectLogger(_ => loggingConfiguration);

    // Database (zelfde context en configuratie als de Identity-service)
    var connectionString = configuration.GetConnectionString("IdentityDb")
        ?? throw new KeyNotFoundException("ConnectionString <<IdentityDb>> not found in application settings");
    serviceCollection.AddDbContext<KampConnectIdentityDbContext>(options =>
    {
        options.UseSqlServer(connectionString);
        options.UseOpenIddict<int>();
    });

    // OpenIddict: enkel de core (managers + EF-stores), geen server nodig
    serviceCollection.AddOpenIddict()
        .AddCore(options => options.UseEntityFrameworkCore()
            .UseDbContext<KampConnectIdentityDbContext>()
            .ReplaceDefaultEntities<int>());

    // Instellingen
    serviceCollection.Configure<RegistrationSettings>(configuration.GetSection("Registration"));

    // Main service
    serviceCollection.AddScoped<ClientRegistrationMainService>();
}
