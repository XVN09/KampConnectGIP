using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Identity.Data.Entity.Claims;
using KampConnect.Identity.Data.Entity.Roles;
using KampConnect.Shared.Logging.Config.Tools;

namespace KampConnect.Identity.Data.Entity.Users;

[Table("sec_user", Schema = "users")]
public class User
{
    #region Properties
    [Key]
    public int user_id { get; set; }

    [MaxLength(100)]
    public string? username { get; set; }

    [MaxLength(255)]
    public string? password_hash { get; set; }

    public DateTime? last_login { get; set; }

    [MaxLength(255)]
    public string? email { get; set; }

    [MaxLength(255)]
    public string? email_normalized { get; set; }

    public DateTime? email_verified_timestamp { get; set; }

    [MaxLength(50)]
    public string? phone_number { get; set; }

    public required bool is_active { get; set; }

    public required int failed_login_count { get; set; }

    public DateTime? locked_until { get; set; }

    public required DateTime creation_timestamp { get; set; }

    public DateTime? modification_timestamp { get; set; }
    #endregion

    #region Navigation Properties
    public ICollection<UserMfa> UserMfas { get; set; } = new List<UserMfa>();
    public ICollection<UserToken> UserTokens { get; set; } = new List<UserToken>();
    public ICollection<LoginAttempt> LoginAttempts { get; set; } = new List<LoginAttempt>();
    public ICollection<ExternalIdentity> ExternalIdentities { get; set; } = new List<ExternalIdentity>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<UserClaim> UserClaims { get; set; } = new List<UserClaim>();
    #endregion

    #region ToString
    public override string ToString()
    {
        return this.DumpObject();
    }
    #endregion
}
