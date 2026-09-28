using KampConnect.Identity.Data.Context.Implentation;
using KampConnect.Identity.Data.Entity.Users;
using KampConnect.Identity.Data.Interface.Repository;
using Microsoft.EntityFrameworkCore;

namespace KampConnect.Identity.Data.Repository.Implementation;

/// <summary>
/// EF Core-implementatie van <see cref="IUserRepository"/>.
/// Enkel queries — geen businessregels (lockout, validatie, …): die zitten in Business.Logic.
/// </summary>
public class UserRepository(KampConnectIdentityDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByIdAsync(int userId, CancellationToken ct = default) =>
        dbContext.Users.FirstOrDefaultAsync(u => u.user_id == userId, ct);

    public Task<User?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken ct = default) =>
        dbContext.Users.FirstOrDefaultAsync(u => u.email_normalized == normalizedEmail, ct);

    public Task<User?> GetByIdWithRolesAsync(int userId, CancellationToken ct = default) =>
        dbContext.Users
            .AsNoTracking()                       // enkel lezen → sneller, geen change tracking
            .Include(u => u.UserRoles)            // koppelingen gebruiker ↔ rol (met tenant_id)
                .ThenInclude(ur => ur.Role)       // de rol zelf (role_code, is_platform_role)
            .FirstOrDefaultAsync(u => u.user_id == userId, ct);

    public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken ct = default) =>
        dbContext.Users.AnyAsync(u => u.email_normalized == normalizedEmail, ct);

    public void Add(User user) => dbContext.Users.Add(user);

    public void AddLoginAttempt(LoginAttempt attempt) => dbContext.LoginAttempts.Add(attempt);

    public Task SaveChangesAsync(CancellationToken ct = default) => dbContext.SaveChangesAsync(ct);
}
