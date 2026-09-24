using KampConnect.Shared.Logging.Config.ClassTypes;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace KampConnect.Shared.Logging.Core.Implementation;

public class KampConnectLogProvider : ILoggerProvider
{
    #region Declarations

    private readonly ConcurrentDictionary<string, KampConnectLogger> _loggers = new();
    private readonly List<CategoryLoggingData> _categoryLoggingConfiguration;
    private readonly LoggingConfigurationData _loggingConfigurationData;

    private readonly Lock _fileWritersLock = new();
    private readonly Dictionary<string, KampConnectLogFileWriter> _fileWriters = new(); // FileWriters for each overriden file path
    private bool _isDisposed;

    #endregion

    #region Constructor

    public KampConnectLogProvider(
        LoggingConfigurationData loggingConfigurationData) : this(
            loggingConfigurationData,
            [])
    { }

    public KampConnectLogProvider(
        LoggingConfigurationData loggingConfigurationData,
        List<CategoryLoggingData> logLevelConfig)
    {
        _loggingConfigurationData = loggingConfigurationData ?? throw new ArgumentNullException(nameof(loggingConfigurationData));
        _categoryLoggingConfiguration = logLevelConfig ?? throw new ArgumentNullException(nameof(logLevelConfig));

        var currDomain = AppDomain.CurrentDomain;
        currDomain.UnhandledException += UnHandledExceptionHandler;
    }

    #endregion

    #region Unhandled Exceptions

    private void UnHandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            try
            {
                var logger = _loggers.GetOrAdd(nameof(KampConnectLogger), CreateLoggerInternal);
                logger.LogCritical(ex, "Unhandled Exception occured");
            }
            catch (Exception)
            {
                // if something went wrong while logging the unhandled exception, we just ignore it ...
            }
        }
    }

    #endregion

    #region ILoggerProvider

    public ILogger CreateLogger(string categoryName) => _loggers.GetOrAdd(categoryName, CreateLoggerInternal);

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        var currDomain = AppDomain.CurrentDomain;
        currDomain.UnhandledException -= UnHandledExceptionHandler;

        lock (_fileWritersLock)
        {
            foreach (var fileWriter in _fileWriters.Values)
            {
                fileWriter.Dispose();
            }
            _fileWriters.Clear();
        }
    }

    #endregion

    #region Private Methods

    private LogLevel GetLogLevelFromCategoryName(string categoryName)
    {
        var logLevel = LogLevel.None;
        var defaultLogLevel = LogLevel.Information;
        var winnersMatchingChars = 0;

        foreach (var categoryLoggingData in _categoryLoggingConfiguration)
        {
            if (categoryLoggingData.IsDefaultCategory)
            {
                defaultLogLevel = categoryLoggingData.LogLevel;
                continue;
            }

            if (categoryName.Contains(categoryLoggingData.CategoryName, StringComparison.OrdinalIgnoreCase))
            {
                if (categoryLoggingData.CategoryName.Length > winnersMatchingChars)
                {
                    winnersMatchingChars = categoryLoggingData.CategoryName.Length;
                    logLevel = categoryLoggingData.LogLevel;
                }
            }
        }

        return winnersMatchingChars > 0 ? logLevel : defaultLogLevel;
    }

    private string? GetPathOverrideFromCategoryName(string categoryName)
    {
        if (LogPathOverride == null)
            return null;

        string? logPathForCategory = null;
        var winnersMatchingChars = 0;

        foreach (var logPathOverrideEntry in LogPathOverride)
        {
            if (categoryName.Contains(logPathOverrideEntry.Key, StringComparison.OrdinalIgnoreCase))
            {
                if (logPathOverrideEntry.Key.Length > winnersMatchingChars)
                {
                    winnersMatchingChars = logPathOverrideEntry.Key.Length;
                    logPathForCategory = logPathOverrideEntry.Value?.ToString();
                }
            }
        }

        return logPathForCategory;
    }

    private KampConnectLogFileWriter GetFileWriterFromCategoryName(string categoryName)
    {
        // Check if there is a path override for the category name
        // If overriden, foresee an archive directory with the same name in it
        var logPathOverride = GetPathOverrideFromCategoryName(categoryName);
        var logPath = logPathOverride ?? _loggingConfigurationData.LogPath;

        // Create the logger or create a new one if it does not exist yet
        lock (_fileWritersLock)
        {
            if (!_fileWriters.TryGetValue(logPath, out KampConnectLogFileWriter? fileWriter))
            {
                var archivePath = _loggingConfigurationData.ArchivePath;
                if (!string.IsNullOrEmpty(logPathOverride) && !string.IsNullOrEmpty(archivePath))
                {
                    var archivePathParts = archivePath.Split("\\", StringSplitOptions.RemoveEmptyEntries);
                    if (archivePathParts.Length > 0)
                    {
                        var archivePathName = archivePathParts[archivePathParts.Length - 1];
                        archivePath = Path.Combine(logPathOverride, archivePathName);
                    }
                }

                fileWriter = new KampConnectLogFileWriter(
                    logPath,
                    _loggingConfigurationData.LogFileName,
                    _loggingConfigurationData.MaxFileSizeInKB,
                    _loggingConfigurationData.MaxFiles,
                    archivePath,
                    _loggingConfigurationData.DaysToKeepArchives);
                fileWriter.Init();
                _fileWriters.Add(logPath, fileWriter);
            }

            return fileWriter;
        }
    }

    private KampConnectLogger CreateLoggerInternal(string categoryName)
    {
        var fileWriter = GetFileWriterFromCategoryName(categoryName);
        return new KampConnectLogger(
            categoryName,
            GetLogLevelFromCategoryName(categoryName),
            message => fileWriter.LogToFile(message));
    }

    #endregion

    #region Public Methods

    public static List<CategoryLoggingData> ParseLogLevelConfig(IDictionary<string, object?> logLevelDictionary)
    {
        if (logLevelDictionary == null)
            throw new ArgumentNullException(nameof(logLevelDictionary));

        var logLevelConfig = new List<CategoryLoggingData>();
        foreach (var logLevelEntry in logLevelDictionary)
        {
            if (Enum.TryParse(typeof(LogLevel), logLevelEntry.Value?.ToString(), true, out var logLevel))
                logLevelConfig.Add(new CategoryLoggingData(logLevelEntry.Key, (LogLevel)logLevel));
        }
        return logLevelConfig;
    }

    public static LogLevel GetMinimumLogLevel(List<CategoryLoggingData> logLevelConfig)
    {
        const LogLevel defaultLogLevel = LogLevel.Information;
        var minLevel = defaultLogLevel;

        foreach (var logLevelEntry in logLevelConfig)
        {
            if ((int)logLevelEntry.LogLevel < (int)minLevel)
                minLevel = logLevelEntry.LogLevel;
        }

        return minLevel;
    }

    #endregion

    #region Properties

    public IDictionary<string, object?>? LogPathOverride { private get; set; } = null;

    #endregion
}
