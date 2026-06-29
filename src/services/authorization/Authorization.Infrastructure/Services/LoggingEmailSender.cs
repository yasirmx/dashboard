using Authorization.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Services;

/// <summary>
/// Development stub. Replace with a real SMTP / SendGrid implementation for production.
/// </summary>
public sealed class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger) => _logger = logger;

    public Task SendPasswordResetAsync(string email, string resetToken, CancellationToken ct)
    {
        _logger.LogInformation(
            "Password reset token for {Email}: {Token} (implement real email delivery in production)",
            email, resetToken);

        return Task.CompletedTask;
    }
}
