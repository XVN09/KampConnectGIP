using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Shared.Logging.Config.Tools;

namespace KampConnect.Identity.Data.Entity.Users;

[Table("sec_user_mfa", Schema = "users")]
public class UserMfa
{
    #region Properties
    [Key]
    public int user_mfa_id { get; set; }

    public required Guid user_id { get; set; }

    [MaxLength(30)]
    public required string mfa_type { get; set; }

    public required byte[] secret_encrypted { get; set; }

    public required bool is_confirmed { get; set; }

    public DateTime? last_used { get; set; }

    public required DateTime creation_timestamp { get; set; }

    public DateTime? modification_timestamp { get; set; }
    #endregion

    #region Navigation Properties
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
