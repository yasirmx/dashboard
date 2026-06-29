namespace Authorization.Application.Abstractions;

public interface ITokenService
{
    string GenerateAccessToken(Guid userId, string email, IEnumerable<string> roles);
    (string rawToken, string tokenHash, DateTime expiresUtc) GenerateRefreshToken();
    Guid? GetUserIdFromToken(string accessToken);
}
