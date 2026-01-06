using System.Net;
using MediaRatingsPlatform.Helpers;
using MediaRatingsPlatform.Interfaces;

namespace MediaRatingsPlatform.Endpoints;

public class LikeEndpoints : IHttpEndpoint
{
    private readonly ILikeService _likeService;
    private readonly IUserService _userService;

    public LikeEndpoints(ILikeService likeService, IUserService userService)
    {
        _likeService = likeService;
        _userService = userService;
    }

    public void RegisterRoutes(Router router)
    {
        router.AddRoute("POST", "/api/ratings/{ratingId}/like", LikeRating);
        router.AddRoute("DELETE", "/api/ratings/{ratingId}/like", UnlikeRating);
    }

    private async Task LikeRating(HttpListenerContext context)
    {
        var user = AuthorizationHelper.AuthorizeRequest(context, _userService);
        if (user == null) return;

        var path = context.Request.Url?.AbsolutePath ?? "";
        var parameters = HttpHelper.ExtractPathParameters("/api/ratings/{ratingId}/like", path);

        if (!parameters.TryGetValue("ratingId", out var idString) || !int.TryParse(idString, out var ratingId))
        {
            HttpHelper.SendJsonResponse(context.Response, 400, "Invalid rating ID");
            return;
        }

        var success = _likeService.LikeRating(user.Id, ratingId);
        if (!success)
        {
             // Own rating or already liked
             HttpHelper.SendJsonResponse(context.Response, 400, "Could not like rating (Already liked or own rating)");
             return;
        }

        HttpHelper.SendJsonResponse(context.Response, 200, "Rating liked");
    }

    private async Task UnlikeRating(HttpListenerContext context)
    {
        var user = AuthorizationHelper.AuthorizeRequest(context, _userService);
        if (user == null) return;

        var path = context.Request.Url?.AbsolutePath ?? "";
        var parameters = HttpHelper.ExtractPathParameters("/api/ratings/{ratingId}/like", path);

        if (!parameters.TryGetValue("ratingId", out var idString) || !int.TryParse(idString, out var ratingId))
        {
            HttpHelper.SendJsonResponse(context.Response, 400, "Invalid rating ID");
            return;
        }

        var success = _likeService.UnlikeRating(user.Id, ratingId);
        if (!success)
        {
            HttpHelper.SendJsonResponse(context.Response, 404, "Like not found");
            return;
        }

        HttpHelper.SendJsonResponse(context.Response, 200, "Rating unliked");
    }
}
