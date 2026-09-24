using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Shared.Logging.Config.Tools;

namespace KampConnect.Identity.Data.Entity.Clients;

[Table("sec_client_value", Schema = "clients")]
public class ClientValue
{
    #region Properties
    [Key]
    public int client_value_id { get; set; }

    public required int client_parameter_id { get; set; }

    public required int authorization_client_id { get; set; }

    [MaxLength(500)]
    public required string parameter_value { get; set; }

    public required DateTime creation_timestamp { get; set; }

    public DateTime? modification_timestamp { get; set; }
    #endregion

    #region Navigation Properties
    [ForeignKey(nameof(client_parameter_id))]
    public ClientParameter? ClientParameter { get; set; }

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
