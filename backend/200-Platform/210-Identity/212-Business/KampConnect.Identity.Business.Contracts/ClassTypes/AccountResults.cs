namespace KampConnect.Identity.Business.Contracts.ClassTypes;

/// <summary>Fouten die de account-endpoints kunnen teruggeven.</summary>
public enum AccountError
{
    None,
    /// <summary>Invoer ongeldig (zie <see cref="AccountResult.ValidationErrors"/>) → 400.</summary>
    Validation,
    /// <summary>E-mailadres al in gebruik → 409.</summary>
    EmailInUse,
    /// <summary>Token ongeldig, verlopen of al gebruikt → 400.</summary>
    InvalidToken
}

/// <summary>
/// Algemeen resultaat van een account-actie.
/// <see cref="ValidationErrors"/> heeft dezelfde vorm als "errors" in ProblemDetails (veldnaam → fouten).
/// </summary>
public record AccountResult(
    bool Succeeded,
    AccountError Error = AccountError.None,
    int? UserId = null,
    IDictionary<string, string[]>? ValidationErrors = null)
{
    public static AccountResult Ok(int? userId = null) => new(true, UserId: userId);
    public static AccountResult Fail(AccountError error) => new(false, error);
    public static AccountResult Invalid(IDictionary<string, string[]> errors) => new(false, AccountError.Validation, ValidationErrors: errors);
}
