using System.IO.Compression;

namespace KampConnect.Shared.Logging.Core.Implementation;

public class KampConnectLogFileWriter(
    string logPath,
    string logFileNameBase,
    int maxLogFileSizeInKb,
    int maxLogFiles,
    string? archivePath,
    int? amountOfDaysToKeepArchives) : IDisposable
{
    #region Declarations

    private readonly string _logPath = logPath ?? throw new ArgumentNullException(nameof(logPath));
    private readonly string _logFileNameBase = logFileNameBase ?? throw new ArgumentNullException(nameof(logFileNameBase));
    private readonly long _maxLogFileSizeInBytes = maxLogFileSizeInKb * 1024L;

    private bool _archiveFiles;

    private readonly Lock _currentLogFileNameLock = new();
    private string _currentLogFileName = string.Empty;
    private readonly Lock _currentLogFileLock = new();
    private StreamWriter? _currentLogFile;
    private long _fileSize;

    private bool _isDisposed;

    #endregion

    #region Dispose

    public void Dispose()
    {
        lock (_currentLogFileLock)
        {
            _isDisposed = true;
            _currentLogFile?.Dispose();
            _currentLogFile = null;
        }
    }

    #endregion

    #region Properties

    public string CurrentLogFileName
    {
        get
        {
            lock (_currentLogFileNameLock)
            {
                if (string.IsNullOrEmpty(_currentLogFileName))
                    _currentLogFileName = Path.Combine(_logPath, $"{_logFileNameBase}_{DateTime.Now:yyMMdd_HHmmssfff}.log");

                return _currentLogFileName;
            }
        }
    }

    #endregion

    #region Public Methods

    public void Init()
    {
        InitFilePath();
    }

    public void LogToFile(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        try
        {
            lock (_currentLogFileLock)
            {
                if (_isDisposed)
                    return;

                var writer = OpenLogfile();
                writer.Write(text);
                _fileSize += (uint)text.Length;
            }
        }
        catch (Exception)
        {
            // ignored
        }
    }

    #endregion

    #region Private Methods

    private void InitFilePath()
    {
        if (string.IsNullOrEmpty(_logPath))
            throw new Exception("LogPath not found in configuration");

        if (!Directory.Exists(_logPath))
            Directory.CreateDirectory(_logPath);

        if (string.IsNullOrEmpty(_logFileNameBase))
            throw new Exception("LogFileName not found in configuration");

        ValidateFileName(_logFileNameBase);

        if (_maxLogFileSizeInBytes <= 0)
            throw new Exception("MaxLogFileSize should be higher than zero");

        if (maxLogFiles <= 0)
            throw new Exception("MaxLogFiles should be higher than zero");

        if (!string.IsNullOrEmpty(archivePath))
        {
            if (!Directory.Exists(archivePath))
                Directory.CreateDirectory(archivePath);

            if ((amountOfDaysToKeepArchives ?? 0) > 0)
            {
                _archiveFiles = true;
            }
        }
    }

    private StreamWriter OpenLogfile()
    {
        lock (_currentLogFileLock)
        {
            if (_currentLogFile == null)
            {
                CleanupLogfiles();

                // open a file
                _currentLogFile = new StreamWriter(CurrentLogFileName, true); // Create if not exists, append if exists
                _currentLogFile.AutoFlush = true;
            }
            else
            {
                // Check if the logfile is within the allowed size
                if (_fileSize > _maxLogFileSizeInBytes) // Max logfile-size reached
                {
                    _fileSize = 0;
                    _currentLogFile.Close();
                    _currentLogFile = null;
                    _currentLogFileName = string.Empty;

                    return OpenLogfile();    // Recurse this function to receive a fresh new logfile
                }
            }

            return _currentLogFile;
        }
    }

    private void CleanupLogfiles()
    {
        CleanupOldestFileInDirectory(
            _logPath,
            "*.log",
            maxLogFiles,
            _archiveFiles);
    }

    private void ArchiveFile(string fileName)
    {
        if (string.IsNullOrEmpty(archivePath))
            return;

        try
        {
            if (!File.Exists(fileName))
                throw new Exception($"File not found to archive: <<{fileName}>>");

            string archiveFileName = Path.Combine(archivePath, $"{DateTime.Today:yyyy-MM-dd}-{_logFileNameBase}.zip");

            // check if file exists, and if not if we should delete an archive before creating a new
            if (!File.Exists(archiveFileName))
            {
                CleanupOldestFileInDirectory(
                    archivePath,
                    "*.zip",
                    amountOfDaysToKeepArchives ?? 1, // keep at least 1 file, but should never happen
                    false);

                using ZipArchive archiveToCreate = ZipFile.Open(archiveFileName, ZipArchiveMode.Create);
            }

            // append file to zip archive
            using ZipArchive archive = ZipFile.Open(archiveFileName, ZipArchiveMode.Update);
            archive.CreateEntryFromFile(fileName, Path.GetFileName(fileName));
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private void CleanupOldestFileInDirectory(
        string path,
        string extensionFilter,
        int maxAmountOfFiles,
        bool archiveBeforeDelete)
    {
        try
        {
            // If maxlogfiles is exceeded : delete oldest files
            // -> Get all files from the log directory
            // -> Delete oldest files
            var files = Directory.GetFiles(path, extensionFilter).ToList();
            if ((files.Count > 0) && (maxAmountOfFiles > 0))
            {
                while (files.Count >= maxAmountOfFiles)
                {
                    var oldestFile = files.First();
                    foreach (var fileName in files)
                    {
                        if (File.GetCreationTimeUtc(fileName) < File.GetCreationTimeUtc(oldestFile))
                            oldestFile = fileName;
                    }

                    try
                    {
                        if (archiveBeforeDelete)
                            ArchiveFile(oldestFile);

                        File.Delete(oldestFile);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    files.Remove(oldestFile);
                }
            }
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private void ValidateFileName(
        string fileName,
        int minLength = 1,
        int maxLength = 255)
    {
        // Check for null, empty string, or invalid length
        if (string.IsNullOrEmpty(fileName) || fileName.Length < minLength || fileName.Length > maxLength)
            throw new InvalidDataException($"fileName <<{fileName}>> is not validated because its length is not in the interval [{minLength}-{maxLength}]");

        // Get invalid file name characters
        char[] invalidChars = Path.GetInvalidFileNameChars();

        // Check if the fileName contains any invalid characters
        foreach (char c in invalidChars)
        {
            if (fileName.Contains(c))
                throw new InvalidDataException($"fileName <<{fileName}>> is not validated because it contains invalid characters");
        }
    }

    #endregion
}
