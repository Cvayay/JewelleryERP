using System.Security.Cryptography;
using System.Text;

namespace JewelleryERP.Helpers;

/// <summary>
/// Secure password hashing utility using PBKDF2 algorithm.
/// </summary>
public static class PasswordHasher
{
    private const int KeySize = 64; // 64 bytes = 512 bits
    private const int Iterations = 100000;

    /// <summary>
    /// Hash a plaintext password using PBKDF2 with a random salt.
    /// Format: salt$hash (both base64-encoded)
    /// </summary>
public static string HashPassword(string password)
{
    byte[] salt = RandomNumberGenerator.GetBytes(16);

    using var algorithm = new Rfc2898DeriveBytes(
        password,
        salt,
        Iterations,
        HashAlgorithmName.SHA256);

    byte[] key = algorithm.GetBytes(KeySize);

    return $"{Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
}
    /// <summary>
    /// Verify a plaintext password against a stored hash.
    /// </summary>
    public static bool VerifyPassword(string password, string hash)
    {
        try
        {
            var parts = hash.Split('$');
            if (parts.Length != 2)
            {
                return false;
            }

            var salt = Convert.FromBase64String(parts[0]);
            var storedKey = Convert.FromBase64String(parts[1]);

            using (var algorithm = new Rfc2898DeriveBytes(
                password,
                salt: salt,
                iterations: Iterations,
                hashAlgorithm: HashAlgorithmName.SHA256))
            {
                var computedKey = algorithm.GetBytes(KeySize);

                return CryptographicOperations.FixedTimeEquals(storedKey, computedKey);
            }
        }
        catch
        {
            // Hash verification failed
            return false;
        }
    }
}
