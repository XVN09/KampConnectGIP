using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Shared.Logging.Config.Tools;

namespace KampConnect.Identity.Data.Entity.PlatformSecurity;

[Table("sec_signing_key", Schema = "platform_security")]
public class SigningKey
{
    #region Properties
    [Key]
    public int signing_key_id { get; set; }

    [MaxLength(100)]
    public required string kid { get; set; }

    [MaxLength(20)]
    public required string algorithm { get; set; }

    [MaxLength(4000)]
    public required string public_key { get; set; }

    [MaxLength(255)]
    public required string private_key_reference { get; set; }

    public required DateTime activated_at { get; set; }

    public DateTime? expires_at { get; set; }

    public DateTime? retired_at { get; set; }
    #endregion

    #region ToString
    public override string ToString()
    {
        return this.DumpObject();
    }
    #endregion
}
