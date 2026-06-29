using MediatR;

namespace Authorization.Application.Queries.Me;

public record MeQuery(string AccessToken) : IRequest<MeResult>;

public record MeResult(Guid UserId, string Email, string[] Roles);
