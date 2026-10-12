using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Identity.Data.Entity.Users;
using KampConnect.Shared.Logging.Config.Tools;
using Microsoft.EntityFrameworkCore;

namespace KampConnect.Identity.Data.Entity.Roles;

/// <summary>
/// Koppeling gebruiker ↔ rol binnen een tenant. tenant_id is null bij een platformrol.
/// </summary>
[Index(nameof(user_id), nameof(role_id), nameof(tenant_id))]
[Table("sec_user_role", Schema = "roles")]
public class UserRole
{
    #region Properties
    [Key]
    public int user_role_id { get; set; }

    public required int role_id { get; set; }

    public required int user_id { get; set; }

    public int? tenant_id { get; set; }

    public required DateTime creation_timestamp { get; set; }

    public DateTime? modification_timestamp { get; set; }
    #endregion

    #region Navigation Properties
    [ForeignKey(nameof(role_id))]
    public Role? Role { get; set; }

    [ForeignKey(nameof(user_id))]
    public User? User { get; set; }
    #endregion

    #region ToString
    public override string ToString()
    {
        return this.DumpObject();
    }
    #endregion
}
