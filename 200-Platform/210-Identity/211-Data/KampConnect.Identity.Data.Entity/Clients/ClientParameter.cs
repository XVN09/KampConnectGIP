using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Shared.Logging.Config.Tools;

namespace KampConnect.Identity.Data.Entity.Clients;

[Table("sec_client_parameter", Schema = "clients")]
public class ClientParameter
{
    #region Properties
    [Key]
    public int client_parameter_id { get; set; }

    [MaxLength(100)]
    public required string client_parameter_name { get; set; }

    [MaxLength(500)]
    public string? client_parameter_description { get; set; }
    #endregion

    #region Navigation Properties
    public ICollection<ClientValue> ClientValues { get; set; } = new List<ClientValue>();
    #endregion

    #region ToString
    public override string ToString()
    {
        return this.DumpObject();
    }
    #endregion
}
