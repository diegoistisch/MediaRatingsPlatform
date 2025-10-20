using MediaRatingsPlatform.Models;

namespace MediaRatingsPlatform.Interfaces;

public interface IUserService
{
    User? RegisterUser(string username, string email, string password);
    AuthenticationResult? AuthenticateUser(string username, string password);
    User? GetUserById(int userId);
    User? GetUserByUsername(string username);
    bool ValidateToken(string token);
    User? GetUserByToken(string token);
}

public class AuthenticationResult
{
    public string Token { get; set; } = "";
    public int UserId { get; set; }
    public string Username { get; set; } = "";
}