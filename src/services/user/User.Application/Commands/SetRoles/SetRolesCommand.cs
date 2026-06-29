using MediatR;

namespace User.Application.Commands.SetRoles;

public record SetRolesCommand(Guid UserId, string[] Roles) : IRequest;
