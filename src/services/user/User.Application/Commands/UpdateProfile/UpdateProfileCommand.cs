using MediatR;

namespace User.Application.Commands.UpdateProfile;

public record UpdateProfileCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string? AvatarUrl,
    string? Department) : IRequest;
