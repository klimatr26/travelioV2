using System.Security.Cryptography;
using System.Text;

namespace TravelioDatabaseConnector.Security;

public static class PasswordHasher
{
    private const int DefaultSaltSize = 16;

    public static string GenerateSalt(int size = DefaultSaltSize)
    {
        var salt = new byte[size];
        RandomNumberGenerator.Fill(salt);
        return Convert.ToBase64String(salt);
    }

    public static string HashPassword(string password, string salt)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);
        ArgumentException.ThrowIfNullOrEmpty(salt);

        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var saltBytes = Convert.FromBase64String(salt);
        var combined = new byte[saltBytes.Length + passwordBytes.Length];

        Buffer.BlockCopy(saltBytes, 0, combined, 0, saltBytes.Length);
        Buffer.BlockCopy(passwordBytes, 0, combined, saltBytes.Length, passwordBytes.Length);
        var hashBytes = SHA256.HashData(combined);
        return Convert.ToBase64String(hashBytes);
    }

    public static (string Hash, string Salt) CreateHashWithSalt(string password)
    {
        var salt = GenerateSalt();
        var hash = HashPassword(password, salt);
        return (hash, salt);
    }

    public static bool VerifyPassword(string password, string hash, string salt)
    {
        ArgumentException.ThrowIfNullOrEmpty(hash);
        ArgumentException.ThrowIfNullOrEmpty(salt);

        var computed = HashPassword(password, salt);
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(hash),
            Convert.FromBase64String(computed));
    }
}
