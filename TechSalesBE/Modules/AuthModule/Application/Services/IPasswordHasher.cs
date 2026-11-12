namespace AuthModule.Application.Services;

public interface IPasswordHasher
{
    string HashPassword(string password);
}