using MediaRatingsPlatform.Interfaces;
using MediaRatingsPlatform.Models;

namespace MediaRatingsPlatform.Services;

public class UserService : IUserService
{
    private readonly IUserRepository userRepository;

    public UserService(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public User? Register(string username, string email, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            return null;

        User? existingUser = userRepository.GetByUsername(username);
        if (existingUser != null)
            return null;

        User user = new User
        {
            Username = username,
            Email = email,
            Password = password
        };

        userRepository.Add(user);
        return user;
    }

    public string? Login(string username, string password)
    {
        User? user = userRepository.GetByUsername(username);
        if (user == null)
            return null;

        if (user.Password != password)
            return null;

        string token = username + "-mrpToken";
        return token;
    }

    public User? GetUserProfile(string username)
    {
        return userRepository.GetByUsername(username);
    }

    public bool ValidateToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            return false;

        // Token format: "username-mrpToken"
        if (!token.EndsWith("-mrpToken"))
            return false;

        var username = token.Replace("-mrpToken", "");
        var user = userRepository.GetByUsername(username);

        return user != null;
    }

    public User? GetUserByToken(string token)
    {
        if (string.IsNullOrEmpty(token) || !token.EndsWith("-mrpToken"))
            return null;

        var username = token.Replace("-mrpToken", "");
        return userRepository.GetByUsername(username);
    }
}