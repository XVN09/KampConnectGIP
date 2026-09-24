using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Shared.Logging.Config.Tools;

namespace KampConnect.Identity.Data.Entity.Clients;

[Table("sec_client_redirect_uri", Schema = "clients")]
public class ClientRedirectUri
{
    #region Properties
    [Key]
    public int client_redirect_uri_id { get; set; }

    public required int authorization_client_id { get; set; }

    [MaxLength(2000)]
    public required string redirect_uri { get; set; }

    public required bool is_active { get; set; }

    public required DateTime creation_timestamp { get; set; }

    public DateTime? modification_timestamp { get; set; }
    #endregion

    #region Navigation Properties
    [ForeignKey(nameof(authorization_client_id))]
    public Client? Client { get; set; }
    #endregion

    #region ToString
    public override string ToString()
    {
        return this.DumpObject();
    }
    #endregion
}
