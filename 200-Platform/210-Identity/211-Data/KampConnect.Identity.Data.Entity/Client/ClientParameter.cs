using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using KampConnect.Shared.Logging.Config.Tools;


namespace KampConnect.Identity.Data.Entity.Client;

[Table("sec_client_parameter", Schema = "client")]
public class ClientParameter
{
    #region Properties
    [Key]
    public Guid client_parameter_id { get; set; }

    [Required]
    public required string client_parameter_name { get; set; }

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
