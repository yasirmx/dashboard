namespace Authorization.Application.Abstractions;

public interface IEmailSender
{
    Task SendPasswordResetAsync(string email, string resetToken, CancellationToken ct = default);
}
