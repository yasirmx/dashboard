namespace Authorization.Application.Commands.Login;

public record LoginResult(string AccessToken, string RefreshToken);
