using System;
using KampConnect.Identity.Data.Context.Implentation;
using KampConnect.Identity.Data.Entity.Users;
using KampConnect.Identity.Data.Interface.Repository;
using Microsoft.EntityFrameworkCore;

namespace KampConnect.Identity.Data.Repository.Implementation;

/// <summary>EF Core-implementatie van <see cref="IUserTokenRepository"/>.</summary>
public class UserTokenRepository(KampConnectIdentityDbContext dbContext) : IUserTokenRepository
{
    public Task<UserToken?> GetUsableAsync(string tokenHash, string purpose, DateTime utcNow, CancellationToken ct = default) =>
        dbContext.UserTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t =>
                t.token_hash == tokenHash &&
                t.token_purpose == purpose &&
                t.consumed_timestamp == null &&          // nog niet gebruikt
                t.expiration_timestamp > utcNow, ct);    // nog niet verlopen

    public Task<List<UserToken>> GetUsableForUserAsync(Guid userId, string purpose, DateTime utcNow, CancellationToken ct = default) =>
        dbContext.UserTokens
            .Where(t =>
                t.user_id == userId &&
                t.token_purpose == purpose &&
                t.consumed_timestamp == null &&
                t.expiration_timestamp > utcNow)
            .ToListAsync(ct);

    public void Add(UserToken token) => dbContext.UserTokens.Add(token);
}
