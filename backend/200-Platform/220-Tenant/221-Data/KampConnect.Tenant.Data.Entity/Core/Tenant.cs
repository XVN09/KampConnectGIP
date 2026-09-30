using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Shared.Logging.Config.Tools;

namespace KampConnect.Tenant.Data.Entity.Core;

[Table("tnt_tenant", Schema = "core")]
public class Tenant
{
    #region Properties
    [Key]
    public int tenant_id { get; set; }

    [Required]
    public required string tenant_name { get; set; }

    [Required]
    public required string tenant_slug { get; set; }

    [Required]
    public required string tenant_status { get; set; }

    public string? primary_email { get; set; }

    public string? primary_phone { get; set; }

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
