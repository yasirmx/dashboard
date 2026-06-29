using MediatR;
using User.Application.Abstractions;
using User.Domain.Events;
using User.Domain.Repositories;

namespace User.Application.Commands.UpdateProfile;

public sealed class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand>
{
    private readonly IProfileRepository _profiles;
    private readonly IQueuePublisher _queue;

    public UpdateProfileCommandHandler(IProfileRepository profiles, IQueuePublisher queue)
    {
        _profiles = profiles;
        _queue = queue;
    }

    public async Task Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _profiles.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"Profile '{request.UserId}' was not found.");

        profile.UpdateProfile(request.FirstName, request.LastName, request.AvatarUrl, request.Department);
        await _profiles.SaveChangesAsync(cancellationToken);

        var evt = new UserProfileUpdated(profile.UserId, profile.FirstName, profile.LastName, profile.AvatarUrl);
        await _queue.PublishAsync("user.profile.updated", evt, cancellationToken);
    }
}
