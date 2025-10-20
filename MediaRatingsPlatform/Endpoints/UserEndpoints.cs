using System.Net;
using MediaRatingsPlatform.Helpers;
using MediaRatingsPlatform.Interfaces;

namespace MediaRatingsPlatform.Endpoints;

public class UserEndpoints : IHttpEndpoint
{
    private readonly IUserService _userService;

    public UserEndpoints(IUserService userService)
    {
        _userService = userService;
    }

    public void RegisterRoutes(Router router)
    {
        router.AddRoute("POST", "/api/users/register", Register);
        router.AddRoute("POST", "/api/users/login", Login);
        router.AddRoute("GET", "/api/users/{id}/profile", GetProfile);
    }

    private async Task Register(HttpListenerContext context)
    {
        try
        {
            var request = await HttpHelper.ReadJsonBody<RegisterRequest>(context.Request);

            if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                HttpHelper.SendJsonResponse(context.Response, 400, "Username and password are required");
                return;
            }

            var user = _userService.RegisterUser(request.Username, request.Email ?? "", request.Password);

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

    private async Task Login(HttpListenerContext context)
    {
        try
        {
            var request = await HttpHelper.ReadJsonBody<LoginRequest>(context.Request);

            if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                HttpHelper.SendJsonResponse(context.Response, 400, "Username and password are required");
                return;
            }

            var loginResult = _userService.AuthenticateUser(request.Username, request.Password);

            if (loginResult == null)
            {
                HttpHelper.SendJsonResponse(context.Response, 401, "Invalid credentials");
                return;
            }

            var response = new LoginResponse
            {
                Token = loginResult.Token,
                UserId = loginResult.UserId,
                Username = loginResult.Username
            };

            HttpHelper.SendJsonResponse(context.Response, 200, response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login error: {ex.Message}");
            HttpHelper.SendJsonResponse(context.Response, 500, "Internal server error");
        }
    }

    private async Task GetProfile(HttpListenerContext context)
    {
        try
        {
            var path = context.Request.Url?.AbsolutePath ?? "";
            var parameters = HttpHelper.ExtractPathParameters("/api/users/{id}/profile", path);

            if (!parameters.TryGetValue("id", out var idString) || !int.TryParse(idString, out var userId))
            {
                HttpHelper.SendJsonResponse(context.Response, 400, "Invalid user ID");
                return;
            }

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

            var user = _userService.GetUserById(userId);

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
