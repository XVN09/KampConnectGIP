using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Shared.Logging.Config.Tools;

namespace KampConnect.Identity.Data.Entity.Claims;

[Table("sec_claim", Schema = "claims")]
public class Claim
{
    #region Properties
    [Key]
    public int claim_id { get; set; }

    [MaxLength(100)]
    public required string claim_type { get; set; }

    [MaxLength(500)]
    public string? claim_description { get; set; }

    public required DateTime creation_timestamp { get; set; }

    public DateTime? modification_timestamp { get; set; }
    #endregion

    #region Navigation Properties
    public ICollection<ClientClaim> ClientClaims { get; set; } = new List<ClientClaim>();
    public ICollection<UserClaim> UserClaims { get; set; } = new List<UserClaim>();
    #endregion

    #region ToString
    public override string ToString()
    {
        return this.DumpObject();
    }
    #endregion
}
