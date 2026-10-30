using KampConnect.Identity.Business.Contracts.Dto;

namespace KampConnect.Identity.Business.Contracts.ClassTypes;

public record InvitationResult(bool Succeded,
    List<InvitationDto> Invitations = null,
    IDictionary<string, string[]>? Errors = null)
{
    public static InvitationResult Success(List<InvitationDto> invitations) => new InvitationResult(true, invitations);
    public static InvitationResult Fail(IDictionary<string, string[]> errors) => new InvitationResult(false, null, errors);
    public static InvitationResult Error(string errorMessage) => new InvitationResult(false, null, new Dictionary<string, string[]> { { "Error", new[] { errorMessage } } });
}
