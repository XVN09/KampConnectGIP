namespace KampConnect.Identity.Business.Contracts.Requests;

// Request-DTO's van de account-endpoints (JSON-body), 1-op-1 met api/identity-service.openapi.yaml.
// Het zijn "records": onveranderlijke dataklassen zonder gedrag.

/// <summary>POST /api/account/register</summary>
/// <param name="Email">E-mailadres (wordt ook de login).</param>
/// <param name="Password">Wachtwoord, minstens 12 tekens.</param>
/// <param name="InvitationToken">
/// Token uit de uitnodigingslink. Bepaalt later rol en organisatie (via de Tenant-service).
/// </param>
public record RegisterRequest(string Email, string Password, string? InvitationToken);

/// <summary>POST /api/account/verify-email</summary>
public record VerifyEmailRequest(string Token);

/// <summary>POST /api/account/forgot-password</summary>
public record ForgotPasswordRequest(string Email);

/// <summary>POST /api/account/reset-password</summary>
public record ResetPasswordRequest(string Token, string NewPassword);
