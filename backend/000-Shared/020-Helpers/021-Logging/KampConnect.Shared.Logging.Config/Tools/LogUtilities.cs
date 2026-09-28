using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text;
using KampConnect.Shared.Logging.Config.Attributes;

namespace KampConnect.Shared.Logging.Config.Tools;

public static class LogUtilities
{
    #region Declarations

    private static readonly Lock PropertyInfoCacheLock = new();

    private static readonly Dictionary<Type, PropertyInfo[]> PropertyInfoCache = new Dictionary<Type, PropertyInfo[]>();
    private static readonly Dictionary<PropertyInfo, MethodInfo> LogMethodOverridesCache = new Dictionary<PropertyInfo, MethodInfo>();
    private static readonly List<PropertyInfo> IgnoredPropertiesCache = new List<PropertyInfo>();

    #endregion

    #region ExtensionMethods

    public static string CreateLogText<T>(
        this List<T> list,
        string separator = ",")
    {
        var sbLogText = new StringBuilder();
        for (var i = 0; i < list.Count; i++)
        {
            sbLogText.Append(list[i]?.ToString() ?? "???");
            if (i < list.Count - 1)
                sbLogText.Append(separator);
        }
        return sbLogText.ToString();
    }

    public static string CreateLogText<TKey, TValue>(
        this Dictionary<TKey, TValue> dictionary,
        string separator = ",") where TKey : notnull
    {
        var sbLogText = new StringBuilder();
        int elementIndex = 0;
        foreach (var kvp in dictionary)
        {
            var key = kvp.Key;
            var value = kvp.Value?.ToString() ?? "???";
            sbLogText.Append($"{key}: <<{value}>>");
            if (elementIndex++ < dictionary.Count - 1)
                sbLogText.Append(separator);
        }

        return sbLogText.ToString();
    }

    public static string DumpObject<T>(
        this T objectToDump)
    {
        // cache all the properties from reflection to avoid multiple calls to these expensive methods
        PropertyInfo[]? propertyInfos;
        lock (PropertyInfoCacheLock)
        {
            if (!PropertyInfoCache.TryGetValue(
                    typeof(T),
                    out propertyInfos))
            {
                propertyInfos = GenerateCachedPropertyInfos<T>();
            }
        }

        if (propertyInfos.Length == 0)
            return string.Empty;

        // now create the dump of the object using the cached properties and StringBuilder
        var sb = new StringBuilder();
        sb.AppendLine($"Dump of data type: {typeof(T)}");
        foreach (var propertyInfo in propertyInfos)
        {
            var value = propertyInfo.GetValue(
                objectToDump);

            if (value == null)
            {
                sb.AppendLine($"{propertyInfo.Name}: <<>>");
                continue;
            }

            // check if the property is ignored for dump
            bool isIgnored;
            lock (PropertyInfoCacheLock)
            {
                isIgnored = IgnoredPropertiesCache.Contains(propertyInfo);
            }

            if (isIgnored)
                continue;

            // check if the normal ToString() method is to be used or another custom method should be used
            MethodInfo? logMethodOverride;
            lock (LogMethodOverridesCache)
            {
                logMethodOverride = LogMethodOverridesCache.GetValueOrDefault(propertyInfo);
            }

            if (logMethodOverride != null)
            {
                sb.AppendLine($"{propertyInfo.Name}: List:");
                sb.AppendLine(logMethodOverride.Invoke(null, [value, "\r\n"])?.ToString() ?? string.Empty);
                continue;
            }

            // apply format if the value is a DateTime
            if (propertyInfo.PropertyType == typeof(DateTime) ||
                propertyInfo.PropertyType == typeof(DateTime?))
            {
                sb.AppendLine($"{propertyInfo.Name}: <<{value:dd/MM/yyyy HH:mm:ss}>>");
                continue;
            }

            // otherwise just use the ToString() method of the value
            sb.AppendLine($"{propertyInfo.Name}: <<{value}>>");
        }

        return sb.ToString();
    }

    public static void LogDebugHighFrequency(
        this ILogger logger,
        Func<string> messageBuilder)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(messageBuilder());
        }
    }

    public static void LogTraceHighFrequency(
        this ILogger logger,
        Func<string> messageBuilder)
    {
        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace(messageBuilder());
        }
    }

    #endregion

    #region Private Static Help Methods

    private static PropertyInfo[] GenerateCachedPropertyInfos<T>()
    {
        lock (PropertyInfoCacheLock)
        {
            var propertyInfos = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var propertyInfo in propertyInfos)
            {
                Type? genericTypeDefinition = propertyInfo.PropertyType.IsGenericType ?
                    propertyInfo.PropertyType.GetGenericTypeDefinition() :
                    null;
                Type[]? genericArguments = genericTypeDefinition != null ?
                    propertyInfo.PropertyType.GetGenericArguments() :
                    null;

                if (genericTypeDefinition != null && genericTypeDefinition == typeof(List<>) &&
                    (genericArguments?.Length ?? 0) == 1)
                {
                    var createLogTextMethodGeneric = typeof(LogUtilities)
                        .GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .FirstOrDefault(method => method.Name == nameof(CreateLogText) &&
                                                  method.IsGenericMethodDefinition &&
                                                  method.GetGenericArguments().Length == 1 &&
                                                  method.GetParameters().Length == 2);

                    if (createLogTextMethodGeneric != null)
                    {
                        var listType = propertyInfo.PropertyType.GetGenericArguments()[0];
                        var createLogTextMethod = createLogTextMethodGeneric.MakeGenericMethod(listType);
                        LogMethodOverridesCache[propertyInfo] = createLogTextMethod;
                    }
                }
                else if (genericTypeDefinition != null && genericTypeDefinition == typeof(Dictionary<,>) &&
                         (genericArguments?.Length ?? 0) == 2)
                {
                    var createLogTextMethodGeneric = typeof(LogUtilities)
                        .GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .FirstOrDefault(method => method.Name == nameof(CreateLogText) &&
                                                  method.IsGenericMethodDefinition &&
                                                  method.GetGenericArguments().Length == 2 &&
                                                  method.GetParameters().Length == 2);

                    if (createLogTextMethodGeneric != null)
                    {
                        var keyType = propertyInfo.PropertyType.GetGenericArguments()[0];
                        var valueType = propertyInfo.PropertyType.GetGenericArguments()[1];
                        var createLogTextMethod = createLogTextMethodGeneric.MakeGenericMethod(keyType, valueType);
                        LogMethodOverridesCache[propertyInfo] = createLogTextMethod;
                    }
                }

                IgnoreForDumpAttribute? ignoreForDumpAttribute = propertyInfo.GetCustomAttribute<IgnoreForDumpAttribute>();
                if (ignoreForDumpAttribute != null)
                {
                    IgnoredPropertiesCache.Add(propertyInfo);
                }
            }

            PropertyInfoCache[typeof(T)] = propertyInfos;
            return propertyInfos;
        }
    }

    #endregion
}
