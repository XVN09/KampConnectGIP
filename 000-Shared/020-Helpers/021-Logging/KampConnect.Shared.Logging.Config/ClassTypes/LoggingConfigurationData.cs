using System.Dynamic;
using KampConnect.Shared.Logging.Config.Tools;

namespace KampConnect.Shared.Logging.Config.ClassTypes;

public class LoggingConfigurationData
{
    #region Properties

    public ExpandoObject? LogLevel { get; set; }
    public ExpandoObject? LogPathOverride { get; set; }

    public string LogPath { get; set; } = string.Empty;
    public string LogFileName { get; set; } = string.Empty;
    public int MaxFiles { get; set; }
    public int MaxFileSizeInKB { get; set; }
    public bool UseConsoleLogging { get; set; }
    public string? ArchivePath { get; set; }
    public int? DaysToKeepArchives { get; set; }

    #endregion

    #region ToString

    public override string ToString()
    {
        return this.DumpObject();
    }

    #endregion
}
