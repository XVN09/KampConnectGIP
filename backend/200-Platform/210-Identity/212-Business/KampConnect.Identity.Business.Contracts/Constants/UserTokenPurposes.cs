namespace KampConnect.Identity.Business.Contracts.Constants;

/// <summary>Mogelijke waarden van token_purpose in users.sec_user_token (max. 30 tekens).</summary>
public static class UserTokenPurposes
{
    public const string EmailVerification = "email_verification";
    public const string PasswordReset = "password_reset";
}
