using System.Net;
using MediaRatingsPlatform.Helpers;
using MediaRatingsPlatform.Interfaces;

namespace MediaRatingsPlatform.Controllers;

public class UserController
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    public async Task Register(HttpListenerContext context)
    {
        try
        {
            var request = await HttpHelper.ReadJsonBody<RegisterRequest>(context.Request);

            if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                HttpHelper.SendJsonResponse(context.Response, 400, "Username and password are required");
                return;
            }

            var user = _userService.Register(request.Username, request.Email ?? "", request.Password);

            if (user == null)
            {
                HttpHelper.SendJsonResponse(context.Response, 409, "Username already exists");
                return;
            }

            var response = new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
            };

            HttpHelper.SendJsonResponse(context.Response, 201, response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Register error: {ex.Message}");
            HttpHelper.SendJsonResponse(context.Response, 500, "Internal server error");
        }
    }

    public async Task Login(HttpListenerContext context)
    {
        try
        {
            var request = await HttpHelper.ReadJsonBody<LoginRequest>(context.Request);

            if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                HttpHelper.SendJsonResponse(context.Response, 400, "Username and password are required");
                return;
            }

            var token = _userService.Login(request.Username, request.Password);

            if (token == null)
            {
                HttpHelper.SendJsonResponse(context.Response, 401, "Invalid credentials");
                return;
            }
            
            var user = _userService.GetUserProfile(request.Username);
            var response = new LoginResponse
            {
                Token = token,
                UserId = user?.Id ?? 0,
                Username = user?.Username ?? ""
            };
            HttpHelper.SendJsonResponse(context.Response, 200, response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login error: {ex.Message}");
            HttpHelper.SendJsonResponse(context.Response, 500, "Internal server error");
        }
    }

    public async Task GetProfile(HttpListenerContext context)
    {
        try
        {
            // Extrahiert id vom path
            var path = context.Request.Url?.AbsolutePath ?? "";
            var parameters = HttpHelper.ExtractPathParameters("/api/users/{id}/profile", path);

            if (!parameters.TryGetValue("id", out var idString) || !int.TryParse(idString, out var userId))
            {
                HttpHelper.SendJsonResponse(context.Response, 400, "Invalid user ID");
                return;
            }

            // Check authentifizierung
            var token = HttpHelper.GetAuthToken(context.Request);
            if (string.IsNullOrEmpty(token))
            {
                HttpHelper.SendJsonResponse(context.Response, 401, "Authentication required");
                return;
            }

            if (!_userService.ValidateToken(token))
            {
                HttpHelper.SendJsonResponse(context.Response, 401, "Invalid or expired token");
                return;
            }

            // Get profile by ID
            var user = _userService.GetUserProfileById(userId);

            if (user == null)
            {
                HttpHelper.SendJsonResponse(context.Response, 404, "User not found");
                return;
            }

            var response = new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
            };

            HttpHelper.SendJsonResponse(context.Response, 200, response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetProfile error: {ex.Message}");
            HttpHelper.SendJsonResponse(context.Response, 500, "Internal server error");
        }
    }
}

// DTOs
public class RegisterRequest
{
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public class LoginRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

public class LoginResponse
{
    public string Token { get; set; } = "";
    public int UserId { get; set; }
    public string Username { get; set; } = "";
}

public class UserResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
}
