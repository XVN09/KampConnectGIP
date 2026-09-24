using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Identity.Data.Entity.Clients;
using KampConnect.Identity.Data.Entity.Users;
using KampConnect.Shared.Logging.Config.Tools;

namespace KampConnect.Identity.Data.Entity.Claims;

[Table("sec_user_claim", Schema = "claims")]
public class UserClaim
{
    #region Properties
    [Key]
    public int user_claim_id { get; set; }

    public required int user_id { get; set; }

    public required int authorization_client_id { get; set; }

    public required int claim_id { get; set; }

    [MaxLength(100)]
    public required string claim_value { get; set; }

    public required DateTime creation_timestamp { get; set; }

    public DateTime? modification_timestamp { get; set; }
    #endregion

    #region Navigation Properties
    [ForeignKey(nameof(user_id))]
    public User? User { get; set; }

    [ForeignKey(nameof(authorization_client_id))]
    public Client? Client { get; set; }

    [ForeignKey(nameof(claim_id))]
    public Claim? Claim { get; set; }
    #endregion

    #region ToString
    public override string ToString()
    {
        return this.DumpObject();
    }
    #endregion
}
