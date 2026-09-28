namespace KampConnect.Identity.Business.Logic.Helpers;

/// <summary>
/// Zet een e-mailadres om naar de vorm in kolom email_normalized.
/// Zo is "Jan@Voorbeeld.be " dezelfde gebruiker als "jan@voorbeeld.be".
/// (Zelfde regel als ASP.NET Identity: trimmen + hoofdletters.)
/// </summary>
internal static class EmailNormalizer
{
    public static string Normalize(string email) => email.Trim().ToUpperInvariant();
}
