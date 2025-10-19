using MediaRatingsPlatform.Models;

namespace MediaRatingsPlatform.Interfaces;

public interface IUserService
{
    User? Register(string username, string email, string password);
    string? Login(string username, string password);
    User? GetUserProfile(string username);
    User? GetUserProfileById(int userId);
    bool ValidateToken(string token);
    User? GetUserByToken(string token);
}