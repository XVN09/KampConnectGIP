using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Shared.Logging.Config.Tools;
using KampConnect.Identity.Data.Entity.Claims;

namespace KampConnect.Identity.Data.Entity.Client;

[Table("sec_clients", Schema = "client")]
public class AuthorizationClient
{
    #region Properties

    [Key]
    public Guid authorization_client_id { get; set; }

    [Required]
    [MaxLength(255)]
    public required string client_id { get; set; }

    [Required]
    public required string client_secret_hash { get; set; }

    [MaxLength(100)]
    public string? client_name { get; set; }

    [Required]
    public bool is_active { get; set; }

    [Required]
    public DateTime created_at { get; set; } = DateTime.UtcNow;

    public DateTime? updated_at { get; set; }

    [Required]
    public required string client_type { get; set; }

    [Required]
    public bool require_pkce { get; set; }

    #endregion

    #region Navigation Properties
    public ICollection<ClientRedirectURI> ClientRedirectURIs { get; set; } = new List<ClientRedirectURI>();
    public ICollection<ClientValue> ClientValues { get; set; } = new List<ClientValue>();
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
