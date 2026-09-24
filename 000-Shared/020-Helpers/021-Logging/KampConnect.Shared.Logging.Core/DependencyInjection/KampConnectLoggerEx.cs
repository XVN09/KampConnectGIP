using KampConnect.Shared.Logging.Config.ClassTypes;
using KampConnect.Shared.Logging.Core.Implementation;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging;

public static class KampConnectLoggerEx
{
    #region Public Methods

    public static void UseKampConnectLogger(
        this IServiceCollection services,
        Func<IServiceCollection, LoggingConfigurationData> getLogConfigurationData)
    {
        LoggingConfigurationData loggingData = getLogConfigurationData(services);
        IDictionary<string, object?>? logLevelDictionary = loggingData.LogLevel;
        IDictionary<string, object?>? logPathOverride = loggingData.LogPathOverride;

        var logProvider = loggingData.BuildLogProvider(
            logLevelDictionary,
            out LogLevel? minimumLevel);
        logProvider.LogPathOverride = logPathOverride;

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddProvider(logProvider);
            builder.ApplyMinimumLogLevel(minimumLevel);
        });
    }

    #endregion

    #region Private Methods

    private static KampConnectLogProvider BuildLogProvider(
        this LoggingConfigurationData loggingData,
        IDictionary<string, object?>? logLevelDictionary,
        out LogLevel? minimumLevel)
    {
        if (logLevelDictionary != null)
        {
            var logLevelConfig = KampConnectLogProvider.ParseLogLevelConfig(logLevelDictionary);
            minimumLevel = KampConnectLogProvider.GetMinimumLogLevel(logLevelConfig);
            return new KampConnectLogProvider(
                loggingData,
                logLevelConfig);
        }

        minimumLevel = null;
        return new KampConnectLogProvider(loggingData);
    }

    private static void ApplyMinimumLogLevel(
        this ILoggingBuilder builder,
        LogLevel? minimumLevel)
    {
        if (minimumLevel != null)
        {
            builder.Services.Configure<LoggerFilterOptions>(options =>
            {
                options.AddFilter<KampConnectLogProvider>(null, minimumLevel.Value);
            });
        }
        else
        {
            builder.SetMinimumLevel(LogLevel.Information); // Default to Information if no minimum level is specified
        }
    }

    #endregion
}
