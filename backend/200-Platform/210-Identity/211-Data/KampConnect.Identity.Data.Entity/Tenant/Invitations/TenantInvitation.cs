using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Identity.Data.Entity.Roles;
using KampConnect.Identity.Data.Entity.Users;
using KampConnect.Shared.Logging.Config.Tools;

namespace KampConnect.Identity.Data.Entity.Tenant.Invitations;

[Table("tenant_invitation", Schema = "roles")]
public class TenantInvitation
{
    #region Properties
    [Key]
    public int tenant_invitation_id { get; set; }

    public required int tenant_id { get; set; }

    [MaxLength(255)]
    public required string email { get; set; }

    public required int role_id { get; set; }

    public required Guid sent_by_user_id { get; set; }

    public Guid? accepted_by_user_id { get; set; }

    public Guid? cancelled_by_user_id { get; set; }

    [Column(TypeName = "nvarchar(20)")]
    public required InvitationStatus status { get; set; }

    [MaxLength(255)]
    public required string token_hash { get; set; }

    public required DateTime creation_timestamp { get; set; }

    public DateTime? modification_timestamp { get; set; }

    public DateTime? accepted_timestamp { get; set; }

    public DateTime? cancelled_timestamp { get; set; }

    public required DateTime expiry_timestamp { get; set; }

    #endregion

    #region Navigation Properties
    [ForeignKey(nameof(tenant_id))]
    public TenantSnapshot? TenantSnapshot { get; set; }

    [ForeignKey(nameof(sent_by_user_id))]
    public User? SentByUser { get; set; }

    [ForeignKey(nameof(accepted_by_user_id))]
    public User? AcceptedByUser { get; set; }

    [ForeignKey(nameof(cancelled_by_user_id))]
    public User? CancelledByUser { get; set; }

    [ForeignKey(nameof(role_id))]
    public Role? Role { get; set; }
    #endregion

    #region ToString
    public override string ToString()
    {
        return this.DumpObject();
    }
    #endregion
}
