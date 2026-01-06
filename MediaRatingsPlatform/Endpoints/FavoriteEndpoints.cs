using System.Net;
using MediaRatingsPlatform.Helpers;
using MediaRatingsPlatform.Interfaces;

namespace MediaRatingsPlatform.Endpoints;

public class FavoriteEndpoints : IHttpEndpoint
{
    private readonly IFavoriteService _favoriteService;
    private readonly IUserService _userService;

    public FavoriteEndpoints(IFavoriteService favoriteService, IUserService userService)
    {
        _favoriteService = favoriteService;
        _userService = userService;
    }

    public void RegisterRoutes(Router router)
    {
        router.AddRoute("POST", "/api/media/{mediaId}/favorite", AddToFavorites);
        router.AddRoute("DELETE", "/api/media/{mediaId}/favorite", RemoveFromFavorites);
        router.AddRoute("GET", "/api/users/{userId}/favorites", GetUserFavorites);
    }

    private async Task AddToFavorites(HttpListenerContext context)
    {
        var user = AuthorizationHelper.AuthorizeRequest(context, _userService);
        if (user == null) return;

        var path = context.Request.Url?.AbsolutePath ?? "";
        var parameters = HttpHelper.ExtractPathParameters("/api/media/{mediaId}/favorite", path);

        if (!parameters.TryGetValue("mediaId", out var idString) || !int.TryParse(idString, out var mediaId))
        {
            HttpHelper.SendJsonResponse(context.Response, 400, "Invalid media ID");
            return;
        }

        var success = _favoriteService.AddToFavorites(user.Id, mediaId);
        if (!success)
        {
             HttpHelper.SendJsonResponse(context.Response, 400, "Could not add to favorites (Media not found or already in favorites)");
             return;
        }

        HttpHelper.SendJsonResponse(context.Response, 200, "Added to favorites");
    }

    private async Task RemoveFromFavorites(HttpListenerContext context)
    {
        var user = AuthorizationHelper.AuthorizeRequest(context, _userService);
        if (user == null) return;

        var path = context.Request.Url?.AbsolutePath ?? "";
        var parameters = HttpHelper.ExtractPathParameters("/api/media/{mediaId}/favorite", path);

        if (!parameters.TryGetValue("mediaId", out var idString) || !int.TryParse(idString, out var mediaId))
        {
            HttpHelper.SendJsonResponse(context.Response, 400, "Invalid media ID");
            return;
        }

        var success = _favoriteService.RemoveFromFavorites(user.Id, mediaId);
        if (!success)
        {
            HttpHelper.SendJsonResponse(context.Response, 404, "Favorite not found");
            return;
        }

        HttpHelper.SendJsonResponse(context.Response, 200, "Removed from favorites");
    }

    private async Task GetUserFavorites(HttpListenerContext context)
    {
        var path = context.Request.Url?.AbsolutePath ?? "";
        var parameters = HttpHelper.ExtractPathParameters("/api/users/{userId}/favorites", path);

        if (!parameters.TryGetValue("userId", out var idString) || !int.TryParse(idString, out var userId))
        {
            HttpHelper.SendJsonResponse(context.Response, 400, "Invalid user ID");
            return;
        }
        
        var favorites = _favoriteService.GetUserFavorites(userId);
        HttpHelper.SendJsonResponse(context.Response, 200, favorites);
    }
}
