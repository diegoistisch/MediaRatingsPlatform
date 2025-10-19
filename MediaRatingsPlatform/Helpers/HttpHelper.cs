using System.Net;
using System.Text;
using System.Text.Json;

namespace MediaRatingsPlatform.Helpers;

public static class HttpHelper
{
    public static async Task<T?> ReadJsonBody<T>(HttpListenerRequest request)
    {
        if (!request.HasEntityBody)
        {
            return default;
        }

        using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
        var body = await reader.ReadToEndAsync();

        try
        {
            return JsonSerializer.Deserialize<T>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            return default;
        }
    }

    public static void SendJsonResponse(HttpListenerResponse response, int statusCode, object? data = null)
    {
        response.StatusCode = statusCode;
        response.ContentType = "application/json";

        string json;
        if (data == null)
        {
            json = "{}";
        }
        else if (data is string str)
        {
            json = JsonSerializer.Serialize(new { message = str });
        }
        else
        {
            json = JsonSerializer.Serialize(data);
        }

        var buffer = Encoding.UTF8.GetBytes(json);
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }

    public static string? GetAuthToken(HttpListenerRequest request)
    {
        var authHeader = request.Headers["Authorization"];
        if (string.IsNullOrEmpty(authHeader))
        {
            return null;
        }

        // format: "Bearer {token}"
        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authHeader.Substring(7);
        }

        return null;
    }

    public static Dictionary<string, string> ExtractPathParameters(string pattern, string path)
    {
        var parameters = new Dictionary<string, string>();
        var patternParts = pattern.Split('/');
        var pathParts = path.Split('/');

        for (int i = 0; i < patternParts.Length; i++)
        {
            if (patternParts[i].StartsWith("{") && patternParts[i].EndsWith("}"))
            {
                var paramName = patternParts[i].Trim('{', '}');
                parameters[paramName] = pathParts[i];
            }
        }

        return parameters;
    }
}
