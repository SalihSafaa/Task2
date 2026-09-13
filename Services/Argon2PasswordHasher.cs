using Isopoh.Cryptography.Argon2;

namespace ProductCatalogApi;

public class Argon2PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        return Argon2.Hash(password);
    }

    public bool Verify(string password, string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        return Argon2.Verify(passwordHash, password);
    }
}