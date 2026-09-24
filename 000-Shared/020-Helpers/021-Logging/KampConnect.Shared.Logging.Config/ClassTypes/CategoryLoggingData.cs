using Microsoft.Extensions.Logging;

namespace KampConnect.Shared.Logging.Config.ClassTypes;

public class CategoryLoggingData(string categoryName, LogLevel logLevel)
{
    #region Declarations

    private const string DefaultCategoryName = "Default";

    #endregion
    

    #region Properties

    public string CategoryName { get; set; } = categoryName;
    public LogLevel LogLevel { get; set; } = logLevel;
    public bool IsDefaultCategory => string.Equals(CategoryName, DefaultCategoryName, StringComparison.OrdinalIgnoreCase);

    #endregion
}
