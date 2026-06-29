using Authorization.Application.Abstractions;
using Authorization.Domain.Entities;
using Authorization.Domain.Events;
using Authorization.Domain.Repositories;
using MediatR;

namespace Authorization.Application.Commands.Register;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand>
{
    private readonly ICredentialRepository _credentials;
    private readonly IPasswordHasher _hasher;
    private readonly IQueuePublisher _queue;

    public RegisterCommandHandler(
        ICredentialRepository credentials,
        IPasswordHasher hasher,
        IQueuePublisher queue)
    {
        _credentials = credentials;
        _hasher = hasher;
        _queue = queue;
    }

    public async Task Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _credentials.ExistsByEmailAsync(request.Email, cancellationToken))
            throw new InvalidOperationException($"Email '{request.Email}' is already registered.");

        var credential = Credential.Create(request.Email, _hasher.Hash(request.Password));

        await _credentials.AddAsync(credential, cancellationToken);
        await _credentials.SaveChangesAsync(cancellationToken);

        var evt = new UserRegistered(credential.Id, credential.Email, request.FirstName, request.LastName);
        await _queue.PublishAsync("user.registered", evt, cancellationToken);
    }
}
