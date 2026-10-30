using System;
using System.Collections.Generic;
using System.Text;
using KampConnect.Identity.Data.Entity.Tenant.Invitations;

namespace KampConnect.Identity.Business.Contracts.Dto;

public class InvitationDto
{
    public int TenantInvitationId { get; set; }
    public string Email { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public InvitationStatus Status { get; set; }
    public Guid SentByUserId { get; set; }
    public DateTime ExpiryTimestamp { get; set; }
    public DateTime CreatedTimestamp { get; set; }

}
