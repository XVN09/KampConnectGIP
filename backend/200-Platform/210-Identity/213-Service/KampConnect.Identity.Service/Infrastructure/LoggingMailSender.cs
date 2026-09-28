using KampConnect.Identity.Business.Contracts.Interfaces;

namespace KampConnect.Identity.Service.Infrastructure;

/// <summary>
/// Development-implementatie van <see cref="IIdentityMailSender"/>: verstuurt niets,
/// maar schrijft de mail (met de link!) naar de log. Zo kan je lokaal registreren en
/// je e-mail bevestigen door de link uit de console te kopiëren.
/// <para>
/// TODO: SmtpMailSender maken voor de eigen SMTP-server en die in Program.cs registreren
/// buiten Development.
/// </para>
/// </summary>
public class LoggingMailSender(ILogger<LoggingMailSender> logger) : IIdentityMailSender
{
    public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
    {
        logger.LogWarning("[MAIL niet verstuurd — development]\nAan: {To}\nOnderwerp: {Subject}\n{Body}", toEmail, subject, htmlBody);
        return Task.CompletedTask;
    }
}
