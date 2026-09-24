using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Identity.Data.Entity.Clients;
using KampConnect.Identity.Data.Entity.Users;
using KampConnect.Shared.Logging.Config.Tools;

namespace KampConnect.Identity.Data.Entity.AuthFlow;

[Table("sec_auth_refresh_token", Schema = "auth_flow")]
public class AuthRefreshToken
{
    #region Properties
    [Key]
    public int authorization_refresh_token_id { get; set; }

    [MaxLength(450)]
    public required string token_hash { get; set; }

    public required int authorization_client_id { get; set; }

    public int? user_id { get; set; }

    public required DateTime expiration_timestamp { get; set; }

    public DateTime? revoked_timestamp { get; set; }

    public int? authorization_code_id { get; set; }

    public required DateTime creation_timestamp { get; set; }

    public DateTime? modification_timestamp { get; set; }

    public DateTime? exchanged_timestamp { get; set; }

    public int? previous_refresh_token_id { get; set; }

    public required Guid token_family_id { get; set; }
    #endregion

    #region Navigation Properties
    [ForeignKey(nameof(authorization_client_id))]
    public Client? Client { get; set; }

    [ForeignKey(nameof(user_id))]
    public User? User { get; set; }

    [ForeignKey(nameof(authorization_code_id))]
    public AuthCode? AuthCode { get; set; }

    [ForeignKey(nameof(previous_refresh_token_id))]
    public AuthRefreshToken? PreviousRefreshToken { get; set; }
    #endregion

    #region ToString
    public override string ToString()
    {
        return this.DumpObject();
    }
    #endregion
}
