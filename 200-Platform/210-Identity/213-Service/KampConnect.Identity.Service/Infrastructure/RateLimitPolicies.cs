namespace KampConnect.Identity.Service;

/// <summary>Namen van de rate-limit policies (geregistreerd in Program.cs).</summary>
public static class RateLimitPolicies
{
    /// <summary>
    /// Voor de loginpagina en /api/account: max. 10 requests per minuut per IP-adres.
    /// Maakt wachtwoorden raden en massaal mails laten versturen een stuk moeilijker.
    /// </summary>
    public const string Auth = "auth";
}
