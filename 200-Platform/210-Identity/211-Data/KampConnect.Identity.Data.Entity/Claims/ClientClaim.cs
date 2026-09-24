using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Shared.Logging.Config.Tools;
using System.Reflection;

namespace KampConnect.Identity.Data.Entity.Claims;

[Table("sec_client_claim", Schema = "claims")]
public class ClientClaim
{
    #region Properties
    [Key]
    public Guid client_claim_id { get; set; }

    [Key]
    public Guid authorization_client_id { get; set; }

    [Key]
    public Guid client_id { get; set; }

    [Required]
    public required string claim_value { get; set; }

    [Required]
    public required DateTime created_at { get; set; }

    public DateTime? updated_at { get; set; }

    #endregion

    #region ToString

    public override string ToString()
    {
        return this.DumpObject();
    }
    #endregion
}
