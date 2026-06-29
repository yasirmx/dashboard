using MediatR;
using User.Application.Abstractions;
using User.Domain.Entities;
using User.Domain.Events;
using User.Domain.Repositories;

namespace User.Application.Commands.SetRoles;

public sealed class SetRolesCommandHandler : IRequestHandler<SetRolesCommand>
{
    private readonly IProfileRepository _profiles;
    private readonly IUserRoleRepository _roles;
    private readonly IQueuePublisher _queue;

    public SetRolesCommandHandler(
        IProfileRepository profiles,
        IUserRoleRepository roles,
        IQueuePublisher queue)
    {
        _profiles = profiles;
        _roles = roles;
        _queue = queue;
    }

    public async Task Handle(SetRolesCommand request, CancellationToken cancellationToken)
    {
        var profile = await _profiles.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"Profile '{request.UserId}' was not found.");

        var roles = request.Roles
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var invalid = roles.Where(r => !Domain.Entities.Roles.IsValid(r)).ToArray();
        if (invalid.Length > 0)
            throw new ArgumentException($"Invalid role(s): {string.Join(", ", invalid)}");

        await _roles.ReplaceRolesAsync(profile.UserId, roles, cancellationToken);
        await _roles.SaveChangesAsync(cancellationToken);

        var evt = new UserRolesChanged(profile.UserId, roles);
        await _queue.PublishAsync("user.roles.changed", evt, cancellationToken);
    }
}
