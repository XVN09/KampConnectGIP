using KampConnect.Identity.Business.Contracts.ClassTypes;
using KampConnect.Identity.Business.Contracts.Requests;

namespace KampConnect.Identity.Business.Contracts.Interfaces;

public interface IInvitationService
{
    Task<InvitationResult> CreateInvitationAsync(CreateInvitationRequest request, CancellationToken ct = default);
}
