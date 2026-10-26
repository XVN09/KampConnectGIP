using System;
using KampConnect.Identity.Business.Contracts.Interfaces;
using OpenIddict.Abstractions;

namespace KampConnect.Identity.Service.Infrastructure;

/// <summary>
/// Trekt alle sessies van een gebruiker in via de managers van OpenIddict.
/// <list type="bullet">
/// <item><b>Autorisaties</b>: de "sessie" tussen gebruiker en app. Intrekken maakt ook de tokens eronder ongeldig.</item>
/// <item><b>Tokens</b>: refresh tokens (en codes) die nog los bestaan.</item>
/// </list>
/// Access tokens zijn JWT's die de andere services zelf valideren: die blijven geldig tot ze
/// verlopen (max. 15 minuten). Daarom is die levensduur bewust kort.
/// </summary>
public class OpenIddictSessionRevoker(
    IOpenIddictAuthorizationManager authorizationManager,
    IOpenIddictTokenManager tokenManager,
    ILogger<OpenIddictSessionRevoker> logger) : ISessionRevoker
{
    public async Task<int> RevokeAllAsync(Guid userId, CancellationToken ct = default)
    {
        var subject = userId.ToString();
        var count = 0;

        // Eerst alles in een lijst verzamelen en pas dan intrekken: tijdens het doorlopen van
        // een query mag je (zonder MARS) geen andere query op dezelfde verbinding uitvoeren.
        var authorizations = new List<object>();
        await foreach (var authorization in authorizationManager.FindBySubjectAsync(subject, ct))
        {
            authorizations.Add(authorization);
        }
        foreach (var authorization in authorizations)
        {
            if (await authorizationManager.TryRevokeAsync(authorization, ct)) count++;
        }

        var tokens = new List<object>();
        await foreach (var token in tokenManager.FindBySubjectAsync(subject, ct))
        {
            tokens.Add(token);
        }
        foreach (var token in tokens)
        {
            if (await tokenManager.TryRevokeAsync(token, ct)) count++;
        }

        logger.LogInformation("{Count} autorisaties/tokens ingetrokken voor gebruiker {UserId}", count, userId);
        return count;
    }
}
