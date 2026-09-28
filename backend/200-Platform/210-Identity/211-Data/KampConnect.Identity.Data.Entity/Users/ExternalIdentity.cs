using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Identity.Data.Entity.PlatformSecurity;
using KampConnect.Shared.Logging.Config.Tools;

namespace KampConnect.Identity.Data.Entity.Users;

[Table("sec_external_identity", Schema = "users")]
public class ExternalIdentity
{
    #region Properties
    [Key]
    public int external_identity_id { get; set; }

    public required int user_id { get; set; }

    [MaxLength(255)]
    public required string issuer { get; set; }

    [MaxLength(255)]
    public required string subject { get; set; }

    [MaxLength(50)]
    public string? provider_label { get; set; }

    public int? tenant_identity_provider_id { get; set; }

    [MaxLength(255)]
    public string? email_at_link { get; set; }

    public DateTime? last_login { get; set; }

    public required DateTime creation_timestamp { get; set; }

    public DateTime? modification_timestamp { get; set; }
    #endregion

    #region Navigation Properties
    [ForeignKey(nameof(user_id))]
    public User? User { get; set; }

    [ForeignKey(nameof(tenant_identity_provider_id))]
    public TenantIdentityProvider? TenantIdentityProvider { get; set; }
    #endregion

    #region ToString
    public override string ToString()
    {
        return this.DumpObject();
    }
    #endregion
}
