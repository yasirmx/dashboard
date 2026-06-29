using Authorization.Application.Abstractions;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace Authorization.Infrastructure.Services;

public sealed class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;
    private static readonly KeyDerivationPrf Prf = KeyDerivationPrf.HMACSHA256;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = KeyDerivation.Pbkdf2(password, salt, Prf, Iterations, KeySize);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    public bool Verify(string password, string hash)
    {
        var parts = hash.Split('.');
        if (parts.Length != 2) return false;

        byte[] salt;
        byte[] storedKey;
        try
        {
            salt = Convert.FromBase64String(parts[0]);
            storedKey = Convert.FromBase64String(parts[1]);
        }
        catch
        {
            return false;
        }

        var key = KeyDerivation.Pbkdf2(password, salt, Prf, Iterations, KeySize);
        return CryptographicOperations.FixedTimeEquals(key, storedKey);
    }
}
