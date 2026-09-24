using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Shared.Logging.Config.Tools;

namespace KampConnect.Identity.Data.Entity.Users;

[Table("sec_login_attempt", Schema = "users")]
public class LoginAttempt
{
    #region Properties
    [Key]
    public int login_attempt_id { get; set; }

    public int? user_id { get; set; }

    [MaxLength(100)]
    public string? attempted_username { get; set; }

    [MaxLength(45)]
    public string? ip_address { get; set; }

    public required bool is_successful { get; set; }

    [MaxLength(50)]
    public string? failure_reason { get; set; }

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
