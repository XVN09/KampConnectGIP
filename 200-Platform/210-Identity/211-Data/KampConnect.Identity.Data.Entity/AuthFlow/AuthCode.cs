using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Identity.Data.Entity.Clients;
using KampConnect.Identity.Data.Entity.Users;
using KampConnect.Shared.Logging.Config.Tools;

namespace KampConnect.Identity.Data.Entity.AuthFlow;

[Table("sec_auth_code", Schema = "auth_flow")]
public class AuthCode
{
    #region Properties
    [Key]
    public int authorization_code_id { get; set; }

    [MaxLength(450)]
    public required string code_hash { get; set; }

    public required int authorization_client_id { get; set; }

    public required int user_id { get; set; }

    [MaxLength(2000)]
    public required string redirect_uri { get; set; }

    [MaxLength(255)]
    public string? code_challenge { get; set; }

    [MaxLength(10)]
    public string? code_challenge_method { get; set; }

    public required DateTime expiration_timestamp { get; set; }

    public DateTime? exchanged_timestamp { get; set; }

    public required DateTime creation_timestamp { get; set; }

    public DateTime? modification_timestamp { get; set; }

    [MaxLength(1000)]
    public string? scope { get; set; }
    #endregion

    #region Navigation Properties
    [ForeignKey(nameof(authorization_client_id))]
    public Client? Client { get; set; }

    [ForeignKey(nameof(user_id))]
    public User? User { get; set; }

    public ICollection<AuthRefreshToken> RefreshTokens { get; set; } = new List<AuthRefreshToken>();
    #endregion

    #region ToString
    public override string ToString()
    {
        return this.DumpObject();
    }
    #endregion
}
