using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Shared.Logging.Config.Tools;

namespace KampConnect.Identity.Data.Entity.Claims;

[Table("sec_claim", Schema = "claims")]
public class Claim
{
    #region Properties
    [Key]
    public Guid claim_id { get; set; }

    [Required]
    public required string claim_type { get; set; }

    public string claim_description { get; set; }

    [Required]
    public required DateTime created_at { get; set; } = DateTime.UtcNow;

    public DateTime? updated_at { get; set; }
    #endregion

    #region Navigation Properties
    public ICollection<Claim> Claims { get; set; } = new List<Claim>();
    public ICollection<UserClaim> Users { get; set; } = new List<UserClaim>();

    #endregion

    #region ToString

    public override string ToString()
    {
        return this.DumpObject();
    }
    #endregion
}
