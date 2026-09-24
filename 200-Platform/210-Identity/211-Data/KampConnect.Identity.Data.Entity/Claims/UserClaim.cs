using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Shared.Logging.Config.Tools;

namespace KampConnect.Identity.Data.Entity.Claims;

[Table("sec_user_claim", Schema = "claims")]
public class UserClaim
{
    #region Properties
    [Key]
    public Guid user_claim_id { get; set; }

    [Key]
    public Guid user_id { get; set; }

    [Key]
    public Guid authorization_client_id { get; set; }

    [Key]
    public Guid claim_id { get; set; }

    [Required]
    public required string claim_value { get; set; }

    [Required]
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
