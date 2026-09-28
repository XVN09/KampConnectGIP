using System.Security.Cryptography;
using System.Text;

namespace KampConnect.Identity.Business.Logic.Helpers;

/// <summary>
/// Maakt eenmalige tokens voor e-mailverificatie en wachtwoordherstel.
/// <list type="bullet">
/// <item>Het <b>token</b> (willekeurig, 32 bytes) gaat in de link in de mail.</item>
/// <item>In de database bewaren we enkel de <b>SHA-256-hash</b>. Een gelekte database bevat dus geen bruikbare tokens.</item>
/// </list>
/// </summary>
internal static class SecureToken
{
    /// <summary>Nieuw token + de hash die in de database komt.</summary>
    public static (string Token, string Hash) Create()
    {
        // 32 bytes uit een cryptografisch veilige bron = 256 bits → onmogelijk te raden.
        var bytes = RandomNumberGenerator.GetBytes(32);

        // Base64Url: veilig in een URL (geen +, / of =).
        var token = Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return (token, Hash(token));
    }

    /// <summary>Hash van een token (hex, 64 tekens) — zo zoeken we het token op in de database.</summary>
    public static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
