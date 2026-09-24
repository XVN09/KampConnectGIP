using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Shared.Logging.Config.Tools;

namespace KampConnect.Identity.Data.Entity.Client;

[Table("sec_client_redirect_uri", Schema = "identity")]
public class ClientRedirectURI
{
    #region Properties

    [Key]
    public Guid client_redirect_uri_id { get; set; }

    [Key]
    public Guid authorization_client_id { get; set; }

    [Required]
    [MaxLength(2000)]
    public required string redirect_uri { get; set; }

    [Required]
    public required bool is_active { get; set; }

    [Required]
    public required DateTime created_at { get; set; } = DateTime.UtcNow;

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
