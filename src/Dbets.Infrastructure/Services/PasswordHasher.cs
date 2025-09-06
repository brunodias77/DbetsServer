using System.Security.Cryptography;
using Dbets.Domain.Services;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Dbets.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 128 / 8; // 128 bits
    private const int KeySize = 256 / 8; // 256 bits
    private const int Iterations = 10000;
    private const KeyDerivationPrf Prf = KeyDerivationPrf.HMACSHA256;

    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        // Generate a random salt
        byte[] salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Hash the password with the salt
        byte[] hash = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: Prf,
            iterationCount: Iterations,
            numBytesRequested: KeySize);

        // Combine salt and hash
        byte[] hashBytes = new byte[SaltSize + KeySize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, KeySize);

        // Convert to base64 string
        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        if (string.IsNullOrEmpty(hashedPassword))
            return false;

        try
        {
            // Convert base64 string back to bytes
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);

            // Verify the hash has the correct length
            if (hashBytes.Length != SaltSize + KeySize)
                return false;

            // Extract the salt
            byte[] salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            // Extract the hash
            byte[] hash = new byte[KeySize];
            Array.Copy(hashBytes, SaltSize, hash, 0, KeySize);

            // Hash the provided password with the extracted salt
            byte[] testHash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: Prf,
                iterationCount: Iterations,
                numBytesRequested: KeySize);

            // Compare the hashes
            return CryptographicOperations.FixedTimeEquals(hash, testHash);
        }
        catch
        {
            return false;
        }
    }
}