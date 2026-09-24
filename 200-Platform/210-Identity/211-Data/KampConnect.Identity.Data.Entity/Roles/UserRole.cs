using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Identity.Data.Entity.Users;
using KampConnect.Shared.Logging.Config.Tools;

namespace KampConnect.Identity.Data.Entity.Roles;

[Table("sec_user_role", Schema = "roles")]
public class UserRole
{
    #region Properties
    [Key]
    public int user_role_id { get; set; }

    public required int client_role_id { get; set; }

    public required int user_id { get; set; }

    public int? tenant_id { get; set; }

    public required DateTime creation_timestamp { get; set; }

    public DateTime? modification_timestamp { get; set; }
    #endregion

    #region Navigation Properties
    [ForeignKey(nameof(client_role_id))]
    public ClientRole? ClientRole { get; set; }

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
