using MediaRatingsPlatform.Interfaces;
using MediaRatingsPlatform.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MediaRatingsPlatform.Services;

public class UserService : IUserService
{
    private readonly IUserRepository userRepository;
    private const string SECRET_KEY = "IchLiebeSWEN";

    public UserService(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    // Generiert einen Token mit HMAC-SHA256 Signatur
    private string GenerateToken(string username, int userId)
    {
        // Payload: {"username":"user1","userId":1,"exp":1234567890}
        var expirationTime = DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds();
        var payload = new
        {
            username = username,
            userId = userId,
            exp = expirationTime
        };

        var payloadJson = JsonSerializer.Serialize(payload);
        var payloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(payloadJson));

        // Signatur erstellen mit HMAC-SHA256
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SECRET_KEY));
        var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadBase64));
        var signatureBase64 = Convert.ToBase64String(signatureBytes);

        // Token Format
        return $"{payloadBase64}.{signatureBase64}";
    }

    // Validiert Token und gibt Payload zurück
    private TokenPayload? ValidateAndDecodeToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            return null;

        var parts = token.Split('.');
        if (parts.Length != 2)
            return null;

        var payloadBase64 = parts[0];
        var signatureBase64 = parts[1];

        // Signatur überprüfen
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SECRET_KEY));
        var expectedSignatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadBase64));
        var expectedSignatureBase64 = Convert.ToBase64String(expectedSignatureBytes);

        // Prüft ob Token manipuliert wurde
        if (signatureBase64 != expectedSignatureBase64)
            return null;

        // Payload decodieren
        try
        {
            var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(payloadBase64));
            var payload = JsonSerializer.Deserialize<TokenPayload>(payloadJson);

            if (payload == null)
                return null;

            // Ablaufzeit prüfen
            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            
            // Token ist abgelaufen
            if (payload.exp < currentTime)
                return null;

            return payload;
        }
        catch
        {
            return null;
        }
    }

    private class TokenPayload
    {
        public string username { get; set; } = "";
        public int userId { get; set; }
        public long exp { get; set; }
    }

    public User? RegisterUser(string username, string email, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
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

    public AuthenticationResult? AuthenticateUser(string username, string password)
    {
        User? user = userRepository.GetByUsername(username);
        if (user == null)
            return null;

        if (user.Password != password)
            return null;

        string token = GenerateToken(username, user.Id);

        return new AuthenticationResult
        {
            Token = token,
            UserId = user.Id,
            Username = user.Username
        };
    }

    public User? GetUserById(int userId)
    {
        return userRepository.GetById(userId);
    }

    public User? GetUserByUsername(string username)
    {
        return userRepository.GetByUsername(username);
    }

    public bool ValidateToken(string token)
    {
        var payload = ValidateAndDecodeToken(token);
        return payload != null;
    }

    public User? GetUserByToken(string token)
    {
        var payload = ValidateAndDecodeToken(token);
        if (payload == null)
            return null;

        return GetUserByUsername(payload.username);
    }
}