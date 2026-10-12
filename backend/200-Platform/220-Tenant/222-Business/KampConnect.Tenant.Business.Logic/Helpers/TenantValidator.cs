using KampConnect.Tenant.Business.Contracts.Constants;

namespace KampConnect.Tenant.Business.Logic.Helpers;

internal sealed class TenantValidator
{
    private readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();

    private readonly List<string> _reservedNames = new List<string>
    {
        "admin",
        "root",
        "system",
        "support",
        "help",
        "contact",
        "info",
        "about",
        "terms",
        "privacy"
    };

    public bool IsValid => _errors.Count == 0;

    // Slug validatie 
    public TenantValidator Slug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            AddError("Slug", TenantErrorCodes.SlugEmpty);
        }
        else if (slug.Length < 3 || slug.Length > 50)
        {
            AddError("Slug", TenantErrorCodes.SlugLength);
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(slug, @"^[a-z0-9]+(-[a-z0-9]+)*$"))
        {
            AddError("Slug", TenantErrorCodes.SlugFormat);
        }
        else if (_reservedNames.Contains(slug))
        {
            AddError("Slug", TenantErrorCodes.SlugReserved);
        }

        return this;
    }

    // Name validatie 
    public TenantValidator Name(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            AddError("Name", TenantErrorCodes.NameEmpty);
        }
        else if (name.Length < 3 || name.Length > 100)
        {
            AddError("Name", TenantErrorCodes.NameLength);
        }
        return this;
    }

    public IDictionary<string, string[]> Errors => _errors.ToDictionary(e => e.Key, e => e.Value.ToArray());

    private void AddError(string field, string message)
    {
        if (!_errors.TryGetValue(field, out var list)) _errors[field] = list = new List<string>();
        list.Add(message);
    }
}
