namespace UserService.Application.Interfaces;

public interface IPasswordHasher
{
    public byte[] HashPassword(string password);
    public bool VerifyPassword(string password, byte[] hash);
}
