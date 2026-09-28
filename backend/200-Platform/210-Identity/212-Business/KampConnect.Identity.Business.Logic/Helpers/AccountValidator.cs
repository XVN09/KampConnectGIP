using System.Net.Mail;

namespace KampConnect.Identity.Business.Logic.Helpers;

/// <summary>
/// Eenvoudige invoercontrole voor e-mail en wachtwoord.
/// Fouten worden verzameld per veld (zelfde vorm als "errors" in ProblemDetails).
/// </summary>
internal sealed class AccountValidator
{
    private readonly Dictionary<string, List<string>> _errors = new();

    public bool IsValid => _errors.Count == 0;

    public AccountValidator Email(string field, string? email)
    {
        // MailAddress controleert de vorm; we eisen ook dat er niets anders in staat
        // (MailAddress aanvaardt bv. ook "Naam <adres>").
        if (string.IsNullOrWhiteSpace(email) ||
            !MailAddress.TryCreate(email.Trim(), out var parsed) ||
            parsed.Address != email.Trim() ||
            email.Length > 255)
        {
            Add(field, "Geef een geldig e-mailadres.");
        }
        return this;
    }

    public AccountValidator Password(string field, string? password, int minimumLength)
    {
        if (string.IsNullOrEmpty(password) || password.Length < minimumLength)
        {
            Add(field, $"Het wachtwoord moet minstens {minimumLength} tekens lang zijn.");
        }
        return this;
    }

    public IDictionary<string, string[]> Errors => _errors.ToDictionary(e => e.Key, e => e.Value.ToArray());

    private void Add(string field, string message)
    {
        if (!_errors.TryGetValue(field, out var list)) _errors[field] = list = new List<string>();
        list.Add(message);
    }
}
