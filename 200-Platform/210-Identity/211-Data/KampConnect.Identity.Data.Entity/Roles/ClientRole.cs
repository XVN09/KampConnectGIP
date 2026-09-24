using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Identity.Data.Entity.Clients;
using KampConnect.Shared.Logging.Config.Tools;

namespace KampConnect.Identity.Data.Entity.Roles;

[Table("sec_client_role", Schema = "roles")]
public class ClientRole
{
    #region Properties
    [Key]
    public int client_role_id { get; set; }

    public required int authorization_client_id { get; set; }

    [MaxLength(100)]
    public required string role_name { get; set; }

    [MaxLength(500)]
    public string? role_description { get; set; }

    public required DateTime creation_timestamp { get; set; }

    public DateTime? modification_timestamp { get; set; }
    #endregion

    #region Navigation Properties
    [ForeignKey(nameof(authorization_client_id))]
    public Client? Client { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    #endregion

    #region ToString
    public override string ToString()
    {
        return this.DumpObject();
    }
    #endregion
}
