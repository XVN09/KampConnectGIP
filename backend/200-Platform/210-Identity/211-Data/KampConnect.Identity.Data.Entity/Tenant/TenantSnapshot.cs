using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Identity.Data.Entity.PlatformSecurity;
using KampConnect.Identity.Data.Entity.Roles;
using KampConnect.Shared.Logging.Config.Tools;

namespace KampConnect.Identity.Data.Entity.Tenant;

[Table("sec_tenant_snapshot", Schema = "roles")]
public class TenantSnapshot
{
    #region Properties

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int tenant_id { get; set; }

    public required string tenant_name { get; set; }

    public required string tenant_slug { get; set; }

    public required DateTime last_synced_timestamp { get; set; }
    #endregion

    #region Navigation Properties
    public ICollection<TenantSnapshot> TenantSnapshots { get; set; } = new List<TenantSnapshot>();
    #endregion

    #region ToString
    public override string ToString()
    {
        return this.DumpObject();
    }
    #endregion
}
