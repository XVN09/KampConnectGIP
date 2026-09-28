using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Shared.Logging.Config.Tools;

namespace KampConnect.Identity.Data.Entity.Roles;

/// <summary>
/// Platformbrede rol (bv. platform_admin, organisation_admin, staff, guardian).
/// De rol is niet meer gekoppeld aan een client; de tenant zit op <see cref="UserRole"/>.
/// </summary>
[Table("sec_role", Schema = "roles")]
public class Role
{
    #region Properties
    [Key]
    public int role_id { get; set; }

    /// <summary>Waarde die in de `roles`-claim van het token komt (uniek).</summary>
    [MaxLength(50)]
    public required string role_code { get; set; }

    [MaxLength(100)]
    public required string role_name { get; set; }

    [MaxLength(500)]
    public string? role_description { get; set; }

    /// <summary>True voor rollen zonder tenant (platformbeheer → claim is_platform_admin).</summary>
    public required bool is_platform_role { get; set; }

    public required DateTime creation_timestamp { get; set; }

    public DateTime? modification_timestamp { get; set; }
    #endregion

    #region Navigation Properties
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    #endregion

    #region ToString
    public override string ToString()
    {
        return this.DumpObject();
    }
    #endregion
}
