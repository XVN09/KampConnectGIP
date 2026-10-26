using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Shared.Logging.Config.Tools;

namespace KampConnect.Identity.Data.Entity.Users;

[Table("sec_user_token", Schema = "users")]
public class UserToken
{
    #region Properties
    [Key]
    public int user_token_id { get; set; }

    public required Guid user_id { get; set; }

    [MaxLength(450)]
    public required string token_hash { get; set; }

    [MaxLength(30)]
    public required string token_purpose { get; set; }

    public required DateTime expiration_timestamp { get; set; }

    public DateTime? consumed_timestamp { get; set; }

    public required DateTime creation_timestamp { get; set; }
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
