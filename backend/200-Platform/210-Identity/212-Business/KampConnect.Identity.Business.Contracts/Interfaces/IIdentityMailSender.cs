namespace KampConnect.Identity.Business.Contracts.Interfaces;

/// <summary>
/// Verstuurt e-mails voor de Identity-service.
/// <para>
/// De logica weet niet HOE er gemaild wordt: de implementatie zit in de Service-laag
/// (nu LoggingMailSender die de mail enkel logt; later een SMTP-implementatie).
/// </para>
/// </summary>
public interface IIdentityMailSender
{
    Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default);
}
