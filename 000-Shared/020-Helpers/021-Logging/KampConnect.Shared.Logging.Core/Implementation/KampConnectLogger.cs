using KampConnect.Shared.Logging.Core.Utilities;
using Microsoft.Extensions.Logging;

namespace KampConnect.Shared.Logging.Core.Implementation;

public class KampConnectLogger(
    string categoryName,
    LogLevel minLogLevel,
    Action<string> logAction) : ILogger
{
    #region ILogger

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel)
    {
        if (logLevel == LogLevel.None)
            return false;

        return (int)minLogLevel <= (int)logLevel;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        try
        {
            var message = LogMessageBuilder.BuildLogMessage(categoryName, logLevel, eventId, state, exception, formatter);
            logAction(message);
        }
        catch (Exception)
        {
            // do not throw exceptions from the logger, just ignore them
            // logging the exception is not applicable here :)
        }
    }

    #endregion
}
