using ECommerceAPI.Services.Interfaces;

namespace ECommerceAPI.Services;

/// <summary>
/// Development email sender — logs to console instead of sending real email.
/// Replace with SmtpEmailSender or SendGridEmailSender for production.
/// </summary>
public class ConsoleEmailSender : IEmailSender
{
    private readonly ILogger<ConsoleEmailSender> _logger;

    public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger) => _logger = logger;

    public Task SendAsync(string to, string subject, string htmlBody)
    {
        _logger.LogInformation(
            """
            ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
            [EMAIL — dev mode, not actually sent]
            To:      {To}
            Subject: {Subject}
            Body:    {Body}
            ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
            """,
            to, subject, htmlBody);
        return Task.CompletedTask;
    }
}
