using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace KampConnect.Shared.Logging.Core.Utilities;

internal static class LogMessageBuilder
{
    #region Public Methods

    public static string BuildLogMessage<TState>(string categoryName, LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        string categoryNameShort = categoryName.Split('.', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? string.Empty;
        var logMessageBuilder = new StringBuilder($"{DateTime.Now:dd/MM/yyyy HH:mm:ss.fff} - {categoryNameShort} - {logLevel} - {Thread.CurrentThread.ManagedThreadId} - ");

        var callingFrameIndex = 1;
        string? callerInfo = null;
        try
        {
            callerInfo = GetCallerInfo(new StackTrace(), logLevel, out callingFrameIndex);
        }
        catch (Exception)
        {
            // ignored
        }

        if (!string.IsNullOrEmpty(callerInfo))
            logMessageBuilder.Append($"{callerInfo} - ");

        logMessageBuilder.AppendLine($"{formatter(state, exception)}");

        if (exception != null)
        {
            logMessageBuilder.AppendLine(GetExceptionMessage(exception));
            try
            {
                logMessageBuilder.AppendLine(new StackTrace(callingFrameIndex, true).ToString());
            }
            catch (Exception)
            {
                // ignored
            }
        }

        return logMessageBuilder.ToString();
    }

    #endregion

    #region Private Methods

    private static string GetCallerInfo(StackTrace? trace, LogLevel logLevel, out int callingFrameIndex)
    {
        callingFrameIndex = 1;

        try
        {
            if (trace == null)
                return string.Empty;

            var frames = trace.GetFrames();
            var logMethodName = string.Empty;
            switch (logLevel)
            {
                case LogLevel.Trace:
                    logMethodName = "LogTrace";
                    break;
                case LogLevel.Debug:
                    logMethodName = "LogDebug";
                    break;
                case LogLevel.Information:
                    logMethodName = "LogInformation";
                    break;
                case LogLevel.Warning:
                    logMethodName = "LogWarning";
                    break;
                case LogLevel.Error:
                    logMethodName = "LogError";
                    break;
                case LogLevel.Critical:
                    logMethodName = "LogCritical";
                    break;
            }

            if (string.IsNullOrEmpty(logMethodName))
                return string.Empty;

            StackFrame? callingFrame = null;
            for (var frameIndex = 0; frameIndex < frames.Length; frameIndex++)
            {
                var frameOfIndex = frames[frameIndex];
                var frameMethodName = frameOfIndex.GetMethod()?.Name;
                if (string.IsNullOrEmpty(frameMethodName))
                    continue;

                if (string.Equals(frameMethodName, logMethodName, StringComparison.OrdinalIgnoreCase))
                {
                    if (++frameIndex < frames.Length)
                        callingFrame = frames[frameIndex];

                    callingFrameIndex = frameIndex;
                    break;
                }
            }

            if (callingFrame == null)
                return string.Empty;

            var frameMethod = callingFrame.GetMethod();
            return GetDisplayNameForDeclaringType(frameMethod);
        }
        catch (Exception) { return string.Empty; }
    }

    private static string GetExceptionMessage(Exception? ex)
    {
        if (ex == null)
            return string.Empty;

        var msg = ex.Message;
        if (ex.InnerException != null)
            msg = $"{msg}\r\n{GetExceptionMessage(ex.InnerException)}";

        return msg;
    }

    private static string GetDisplayNameForDeclaringType(MethodBase? methodBase)
    {
        if (methodBase == null)
            return string.Empty;

        try
        {
            const string asyncMethodName = "MoveNext";
            if (methodBase.Name == asyncMethodName)
            {
                var declaringTypeName = methodBase.DeclaringType?.Name;
                if (!string.IsNullOrEmpty(declaringTypeName) && declaringTypeName.StartsWith('<'))
                {
                    var fullDeclaringTypeName = methodBase.DeclaringType?.FullName;
                    if (!string.IsNullOrEmpty(fullDeclaringTypeName) && fullDeclaringTypeName.EndsWith(declaringTypeName))
                    {
                        // This is an async method, try to get the name of the actual method that called the async method
                        // fullDeclaringTypeName format is "<FullQualifiedClassName>+<{CallerMethodName}>d__{Number}"
                        var fullQualifiedClassName = fullDeclaringTypeName.Split('+', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                        if (!string.IsNullOrEmpty(fullQualifiedClassName))
                        {
                            var callerMethodName = fullQualifiedClassName
                                .Split('.', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                            int endPosition = declaringTypeName.IndexOf('>');
                            if (endPosition > 0)
                            {
                                var methodName = declaringTypeName.Substring(1, endPosition - 1);
                                if (!string.IsNullOrEmpty(callerMethodName) && !string.IsNullOrWhiteSpace(methodName))
                                    return $"{callerMethodName}.{methodName}";
                            }
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
            // ignored
        }

        return $"{methodBase.DeclaringType?.Name}.{methodBase.Name}";
    }

    #endregion
}
