using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Identity.Data.Entity.Users;
using KampConnect.Shared.Logging.Config.Tools;

namespace KampConnect.Identity.Data.Entity.PlatformSecurity;

[Table("sec_tenant_identity_provider", Schema = "platform_security")]
public class TenantIdentityProvider
{
    #region Properties
    [Key]
    public int tenant_identity_provider_id { get; set; }

    public required int tenant_id { get; set; }

    [MaxLength(255)]
    public required string issuer { get; set; }

    [MaxLength(255)]
    public required string provider_client_id { get; set; }

    [MaxLength(255)]
    public string? email_domain { get; set; }

    public required bool is_active { get; set; }

    public required DateTime creation_timestamp { get; set; }

    public DateTime? modification_timestamp { get; set; }
    #endregion

    #region Navigation Properties
    public ICollection<ExternalIdentity> ExternalIdentities { get; set; } = new List<ExternalIdentity>();
    #endregion

    #region ToString
    public override string ToString()
    {
        return this.DumpObject();
    }
    #endregion
}
