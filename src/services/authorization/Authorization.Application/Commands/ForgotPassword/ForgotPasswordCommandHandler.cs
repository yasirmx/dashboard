using Authorization.Application.Abstractions;
using Authorization.Domain.Repositories;
using MediatR;

namespace Authorization.Application.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand>
{
    private readonly ICredentialRepository _credentials;
    private readonly IEmailSender _email;

    public ForgotPasswordCommandHandler(ICredentialRepository credentials, IEmailSender email)
    {
        _credentials = credentials;
        _email = email;
    }

    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var credential = await _credentials.GetByEmailAsync(request.Email, cancellationToken);

        // Always return 204 even when email is not found to prevent user enumeration.
        if (credential is null)
            return;

        var resetToken = Guid.NewGuid().ToString("N");
        credential.SetPasswordResetToken(resetToken, DateTime.UtcNow.AddHours(1));
        await _credentials.SaveChangesAsync(cancellationToken);

        await _email.SendPasswordResetAsync(credential.Email, resetToken, cancellationToken);
    }
}
