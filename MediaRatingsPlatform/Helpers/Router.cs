using System.Net;

namespace MediaRatingsPlatform.Helpers;

public class Router
{
    private readonly Dictionary<string, Func<HttpListenerContext, Task>> _routes;

    public Router()
    {
        _routes = new Dictionary<string, Func<HttpListenerContext, Task>>();
    }

    public void AddRoute(string method, string path, Func<HttpListenerContext, Task> handler)
    {
        var key = $"{method}:{path}";
        _routes[key] = handler;
    }

    public async Task<bool> RouteRequest(HttpListenerContext context)
    {
        var method = context.Request.HttpMethod;
        var path = context.Request.Url?.AbsolutePath ?? "/";

        // versucht exakten match zu finden
        var key = $"{method}:{path}";
        if (_routes.TryGetValue(key, out var handler))
        {
            await handler(context);
            return true;
        }

        // pattern matching für pfade mit parametern
        foreach (var route in _routes)
        {
            var routeParts = route.Key.Split(':');
            if (routeParts.Length != 2) continue;

            var routeMethod = routeParts[0];
            var routePath = routeParts[1];

            if (routeMethod != method) continue;

            if (MatchesPattern(routePath, path))
            {
                await route.Value(context);
                return true;
            }
        }

        return false;
    }

    private bool MatchesPattern(string pattern, string path)
    {
        var patternParts = pattern.Split('/');
        var pathParts = path.Split('/');

        if (patternParts.Length != pathParts.Length) return false;

        for (int i = 0; i < patternParts.Length; i++)
        {
            if (patternParts[i].StartsWith("{") && patternParts[i].EndsWith("}"))
            {
                // Parameter platzhalter, akzeptiert alles
                continue;
            }

            if (patternParts[i] != pathParts[i])
            {
                return false;
            }
        }

        return true;
    }

    public Dictionary<string, string> ExtractPathParameters(string pattern, string path)
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
