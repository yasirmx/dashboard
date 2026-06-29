using Authorization.Application.Abstractions;
using Authorization.Domain.Repositories;
using MediatR;

namespace Authorization.Application.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand>
{
    private readonly ICredentialRepository _credentials;
    private readonly IPasswordHasher _hasher;

    public ResetPasswordCommandHandler(ICredentialRepository credentials, IPasswordHasher hasher)
    {
        _credentials = credentials;
        _hasher = hasher;
    }

    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var credential = await _credentials.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new InvalidOperationException("Invalid reset request.");

        if (!credential.IsPasswordResetTokenValid(request.Token))
            throw new InvalidOperationException("Reset token is invalid or expired.");

        credential.ResetPassword(_hasher.Hash(request.NewPassword));
        await _credentials.SaveChangesAsync(cancellationToken);
    }
}
