using System.Net;
using System.Text;

namespace MediaRatingsPlatform;

public class HttpServer
{
    private readonly HttpListener _listener;
    private readonly string _url;
    private bool _isRunning;

    public HttpServer(string url = "http://localhost:8080/")
    {
        _url = url;
        _listener = new HttpListener();
        _listener.Prefixes.Add(_url);
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

    private void HandleRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        try
        {
            Console.WriteLine($"{request.HttpMethod} {request.Url?.AbsolutePath}");
            
            var path = request.Url?.AbsolutePath ?? "/";
            var method = request.HttpMethod;

            // Route handling
            if (path == "/" && method == "GET")
            {
                SendResponse(response, 200, "Media Ratings Platform API is running");
            }
            else
            {
                SendResponse(response, 404, "Endpoint not found");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request handling error: {ex.Message}");
            SendResponse(response, 500, "Internal server error");
        }
    }

    private void SendResponse(HttpListenerResponse response, int statusCode, string content)
    {
        response.StatusCode = statusCode;
        response.ContentType = "application/json";

        var buffer = Encoding.UTF8.GetBytes(content);
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }
}
