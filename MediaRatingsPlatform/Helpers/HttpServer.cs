using System.Net;
using MediaRatingsPlatform.Controllers;
using MediaRatingsPlatform.Interfaces;

namespace MediaRatingsPlatform.Helpers;

public class HttpServer
{
    private readonly HttpListener _listener;
    private readonly string _url;
    private bool _isRunning;
    private readonly Router _router;
    private readonly UserController _userController;

    public HttpServer(string url, IUserService userService)
    {
        _url = url;
        _listener = new HttpListener();
        _listener.Prefixes.Add(_url);
        _router = new Router();
        _userController = new UserController(userService);

        ConfigureRoutes();
    }

    private void ConfigureRoutes()
    {
        // User endpoints
        _router.AddRoute("POST", "/api/users/register", _userController.Register);
        _router.AddRoute("POST", "/api/users/login", _userController.Login);
        _router.AddRoute("GET", "/api/users/{id}/profile", _userController.GetProfile);
    }

    public void Start()
    {
        _listener.Start();
        _isRunning = true;
        Console.WriteLine($"Server started on {_url}");

        Task.Run(() => Listen());
    }

    public void Stop()
    {
        _isRunning = false;
        _listener.Stop();
        Console.WriteLine("Server stopped");
    }

    private async Task Listen()
    {
        while (_isRunning)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                _ = Task.Run(() => HandleRequest(context));
            }
            catch (Exception ex)
            {
                if (_isRunning)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }

    private async Task HandleRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        try
        {
            Console.WriteLine($"{request.HttpMethod} {request.Url?.AbsolutePath}");

            var path = request.Url?.AbsolutePath ?? "/";
            var method = request.HttpMethod;

            // Root endpoint
            if (path == "/" && method == "GET")
            {
                HttpHelper.SendJsonResponse(response, 200, "Media Ratings Platform API is running");
                return;
            }

            // versucht den request an die richtige handler funktion weiterzuleiten
            var routed = await _router.RouteRequest(context);

            if (!routed)
            {
                HttpHelper.SendJsonResponse(response, 404, "Endpoint not found");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request handling error: {ex.Message}");
            HttpHelper.SendJsonResponse(response, 500, "Internal server error");
        }
    }
}
