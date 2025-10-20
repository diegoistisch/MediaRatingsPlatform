using System.Net;
using MediaRatingsPlatform.Interfaces;
using MediaRatingsPlatform.Models;

namespace MediaRatingsPlatform.Helpers;

public static class AuthorizationHelper
{
    public static User? AuthorizeRequest(HttpListenerContext context, IUserService userService)
    {
        var token = HttpHelper.GetAuthToken(context.Request);
        if (string.IsNullOrEmpty(token))
        {
            HttpHelper.SendJsonResponse(context.Response, 401, "Authentication required");
            return null;
        }

        if (!userService.ValidateToken(token))
        {
            HttpHelper.SendJsonResponse(context.Response, 401, "Invalid or expired token");
            return null;
        }

        return userService.GetUserByToken(token);
    }
}
