using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Identity.Data.Entity.AuthFlow;
using KampConnect.Identity.Data.Entity.Claims;
using KampConnect.Identity.Data.Entity.Roles;
using KampConnect.Shared.Logging.Config.Tools;

namespace KampConnect.Identity.Data.Entity.Clients;

[Table("sec_client", Schema = "clients")]
public class Client
{
    #region Properties
    [Key]
    public int authorization_client_id { get; set; }

    [MaxLength(255)]
    public required string client_id { get; set; }

    [MaxLength(255)]
    public string? client_secret_hash { get; set; }

    [MaxLength(100)]
    public required string client_name { get; set; }

    public required bool is_active { get; set; }

    public required DateTime creation_timestamp { get; set; }

    public DateTime? modification_timestamp { get; set; }

    [MaxLength(30)]
    public required string client_type { get; set; }

    public required bool require_pkce { get; set; }
    #endregion

    #region Navigation Properties
    public ICollection<ClientRedirectUri> RedirectUris { get; set; } = new List<ClientRedirectUri>();
    public ICollection<ClientValue> ClientValues { get; set; } = new List<ClientValue>();
    public ICollection<ClientRole> ClientRoles { get; set; } = new List<ClientRole>();
    public ICollection<ClientClaim> ClientClaims { get; set; } = new List<ClientClaim>();
    public ICollection<UserClaim> UserClaims { get; set; } = new List<UserClaim>();
    public ICollection<AuthCode> AuthCodes { get; set; } = new List<AuthCode>();
    public ICollection<AuthRefreshToken> AuthRefreshTokens { get; set; } = new List<AuthRefreshToken>();
    #endregion

    #region ToString
    public override string ToString()
    {
        return this.DumpObject();
    }
    #endregion
}
