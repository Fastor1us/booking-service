using System.Security.Cryptography;
using System.Text;
using BookingApi.Application.Interfaces;

namespace BookingApi.Infrastructure.Security;

public class Sha256PasswordHasher : IPasswordHasher
{
    public byte[] HashPassword(string password)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes(password));
    }

    public bool VerifyPassword(string password, byte[] hash)
    {
        var computedHash = HashPassword(password);

        return CryptographicOperations
            .FixedTimeEquals(computedHash, hash);
    }
}
