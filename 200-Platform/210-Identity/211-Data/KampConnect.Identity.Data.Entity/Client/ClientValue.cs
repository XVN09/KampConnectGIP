using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Shared.Logging.Config.Tools;
using KampConnect.Identity.Data.Entity.Claims;


namespace KampConnect.Identity.Data.Entity.Client;

[Table("sec_client_value", Schema = "identity")]
public class ClientValue
{
    #region Properties
    [Key]
    public Guid client_value_id { get; set; }

    [Key]
    public Guid client_parameter_id { get; set; }

    [Key]
    public Guid authorization_client_id { get; set; }

    [Required]
    [MaxLength(500)]
    public required string parameter_value { get; set; }

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
