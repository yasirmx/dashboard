using MediatR;
using User.Application.Abstractions;
using User.Domain.Events;
using User.Domain.Repositories;

namespace User.Application.Commands.Deactivate;

public sealed class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand>
{
    private readonly IProfileRepository _profiles;
    private readonly IQueuePublisher _queue;

    public DeactivateUserCommandHandler(IProfileRepository profiles, IQueuePublisher queue)
    {
        _profiles = profiles;
        _queue = queue;
    }

    public async Task Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var profile = await _profiles.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"Profile '{request.UserId}' was not found.");

        profile.Deactivate();
        await _profiles.SaveChangesAsync(cancellationToken);

        var evt = new UserDeactivated(profile.UserId);
        await _queue.PublishAsync("user.profile.deactivated", evt, cancellationToken);
    }
}
