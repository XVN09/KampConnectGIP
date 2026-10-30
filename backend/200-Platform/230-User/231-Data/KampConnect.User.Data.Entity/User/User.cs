using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Shared.Logging.Config.Tools;

namespace KampConnect.Identity.Data.Entity.Client;

[Table("usr_user", Schema = "user")]
public class User
{
    #region Properties
    [Key]
    public Guid user_id { get; set; }

    public Guid auth_user_id { get; set; }

    public string first_name { get; set; }

    public string last_name { get; set; }

    public DateOnly birth_date { get; set; }

    [MaxLength(255)]
    public string preferred_language { get; set; }

    public Guid avatar_file_id { get; set; }

    public 

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
