namespace KampConnect.Identity.Business.Contracts.ClassTypes;

/// <summary>Waarom een login mislukte.</summary>
public enum LoginFailure
{
    None,
    /// <summary>E-mail of wachtwoord fout. Bewust één reden: zo verklap je niet of een account bestaat.</summary>
    InvalidCredentials,
    /// <summary>Te veel mislukte pogingen; zie <see cref="LoginResult.LockedUntilUtc"/>.</summary>
    LockedOut,
    /// <summary>Account is gedeactiveerd.</summary>
    Inactive,
    /// <summary>E-mailadres nog niet bevestigd (en RequireVerifiedEmail staat aan).</summary>
    EmailNotVerified
}

/// <summary>Resultaat van <c>ILoginService.ValidateCredentialsAsync</c>.</summary>
public record LoginResult(bool Succeeded, Guid? UserId, LoginFailure Failure, DateTime? LockedUntilUtc = null)
{
    public static LoginResult Success(Guid userId) => new(true, userId, LoginFailure.None);
    public static LoginResult Fail(LoginFailure failure, DateTime? lockedUntilUtc = null) => new(false, null, failure, lockedUntilUtc);
}
