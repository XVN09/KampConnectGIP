using KampConnect.Identity.Business.Contracts.ClassTypes;
using KampConnect.Identity.Business.Contracts.Interfaces;
using KampConnect.Identity.Business.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace KampConnect.Identity.Service.Controllers;

/// <summary>
/// /api/account — registratie en wachtwoordbeheer (zie api/identity-service.openapi.yaml).
/// <para>
/// De controller doet enkel HTTP: request binnen, service aanroepen, resultaat omzetten naar
/// de juiste statuscode. Alle regels zitten in <see cref="IAccountService"/> (Business.Logic).
/// </para>
/// </summary>
[ApiController]                         // automatische 400 bij onleesbare JSON + ProblemDetails
[Route("api/account")]
[EnableRateLimiting(RateLimitPolicies.Auth)] // beperkt het aantal pogingen per IP (zie Program.cs)
public class AccountController(IAccountService accountService) : ControllerBase
{
    /// <summary>Account aanmaken → 201 { userId } · 400 validatie · 409 e-mail in gebruik.</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        var result = await accountService.RegisterAsync(request, ct);
        if (result.Succeeded)
        {
            // 201 Created met de id van het nieuwe account in de body
            return StatusCode(StatusCodes.Status201Created, new { userId = result.UserId });
        }
        return ToErrorResult(result);
    }

    /// <summary>E-mailadres bevestigen → 204 · 400 token ongeldig/verlopen.</summary>
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request, CancellationToken ct)
    {
        var result = await accountService.VerifyEmailAsync(request, ct);
        return result.Succeeded ? NoContent() : ToErrorResult(result);
    }

    /// <summary>Herstelmail aanvragen → altijd 204 (ook als het account niet bestaat).</summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken ct)
    {
        await accountService.ForgotPasswordAsync(request, ct);
        return NoContent();
    }

    /// <summary>Nieuw wachtwoord instellen → 204 · 400 validatie of token ongeldig.</summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken ct)
    {
        var result = await accountService.ResetPasswordAsync(request, ct);
        return result.Succeeded ? NoContent() : ToErrorResult(result);
    }

    /// <summary>Zet een mislukt <see cref="AccountResult"/> om naar ProblemDetails met de juiste statuscode.</summary>
    private IActionResult ToErrorResult(AccountResult result) => result.Error switch
    {
        AccountError.Validation => ValidationProblem(new ValidationProblemDetails(result.ValidationErrors ?? new Dictionary<string, string[]>())
        {
            Title = "Validatiefout",
            Status = StatusCodes.Status400BadRequest
        }),
        AccountError.EmailInUse => Problem(title: "E-mailadres al in gebruik", statusCode: StatusCodes.Status409Conflict),
        AccountError.InvalidToken => Problem(title: "Token ongeldig of verlopen", statusCode: StatusCodes.Status400BadRequest),
        _ => Problem(statusCode: StatusCodes.Status500InternalServerError)
    };
}
