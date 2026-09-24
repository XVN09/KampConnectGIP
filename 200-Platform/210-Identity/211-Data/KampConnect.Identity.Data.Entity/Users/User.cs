using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Shared.Logging.Config.Tools;

namespace KampConnect.Identity.Data.Entity.Users;

[Table("sec_user", Schema = "users")]
public class User
{
    #region Properties
    [Key]
    public int user_id { get; set; }

    public string? username { get; set; }

    public string? passwordHash { get; set; }

    public DateTime? last_login { get; set; }

    public string? email { get; set; }

    public string? email_normalized { get; set; }

    public DateTime? email_verified_timestamp { get; set; }

    public string? phone_number { get; set; }

    [Required]
    public required bool is_active { get; set; }

    [Required]
    public required int failed_login_count { get; set; }

    public DateTime? locked_until { get; set; }

    [Required]
    public required DateTime created_at { get; set; }

    public DateTime? updated_at { get; set; }
    #endregion

    #region Navigation Properties
    public ICollection<UserMfa> UserMfas { get; set; } = new List<UserMfa>();

    #endregion

    #region ToString

    public override string ToString()
    {
        return this.DumpObject();
    }
    #endregion
}
