using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Shared.Logging.Config.Tools;

namespace KampConnect.Identity.Data.Entity.Users;

[Table("sec_user_mfa", Schema = "users")]
public class UserMfa
{
    #region Properties
    [Key]
    public Guid user_mfa_id { get; set; }

    [Key]
    public Guid user_id { get; set; }

    [Required]
    public required string mfa_type { get; set; }

    [Required]
    public required string secret_encrypted { get; set; }

    [Required]
    public required bool is_confirmed { get; set; }

    public DateTime? last_used { get; set; }

    [Required]
    public required DateTime created_at { get; set; }

    public DateTime? updated_at { get; set; }

    #endregion

    #region Navigation Properties

    #endregion

    #region ToString

    public override string ToString()
    {
        return this.DumpObject();
    }
    #endregion
}
