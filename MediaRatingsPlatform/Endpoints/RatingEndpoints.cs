using System.Net;
using MediaRatingsPlatform.Helpers;
using MediaRatingsPlatform.Interfaces;
using MediaRatingsPlatform.Models;

namespace MediaRatingsPlatform.Endpoints;

public class RatingEndpoints : IHttpEndpoint
{
    private readonly IRatingService _ratingService;
    private readonly IUserService _userService;

    public RatingEndpoints(IRatingService ratingService, IUserService userService)
    {
        _ratingService = ratingService;
        _userService = userService;
    }

    public void RegisterRoutes(Router router)
    {
        router.AddRoute("GET", "/api/media/{mediaId}/ratings", GetMediaRatings);
        router.AddRoute("POST", "/api/media/{mediaId}/ratings", CreateRating);
        router.AddRoute("GET", "/api/users/{userId}/ratings", GetUserRatings);
        router.AddRoute("PUT", "/api/ratings/{id}", UpdateRating);
        router.AddRoute("DELETE", "/api/ratings/{id}", DeleteRating);
        router.AddRoute("POST", "/api/ratings/{id}/confirm", ConfirmRating);
    }

    private async Task GetMediaRatings(HttpListenerContext context)
    {
        var path = context.Request.Url?.AbsolutePath ?? "";
        var parameters = HttpHelper.ExtractPathParameters("/api/media/{mediaId}/ratings", path);

        if (!parameters.TryGetValue("mediaId", out var idString) || !int.TryParse(idString, out var mediaId))
        {
            HttpHelper.SendJsonResponse(context.Response, 400, "Invalid media ID");
            return;
        }
        
        // Optional: Filter logic could go here or in Service
        var ratings = _ratingService.GetMediaRatings(mediaId);
        HttpHelper.SendJsonResponse(context.Response, 200, ratings);
    }

    private async Task CreateRating(HttpListenerContext context)
    {
        var user = AuthorizationHelper.AuthorizeRequest(context, _userService);
        if (user == null) return;

        var path = context.Request.Url?.AbsolutePath ?? "";
        var parameters = HttpHelper.ExtractPathParameters("/api/media/{mediaId}/ratings", path);

        if (!parameters.TryGetValue("mediaId", out var idString) || !int.TryParse(idString, out var mediaId))
        {
            HttpHelper.SendJsonResponse(context.Response, 400, "Invalid media ID");
            return;
        }

        var request = await HttpHelper.ReadJsonBody<CreateRatingRequest>(context.Request);
        if (request == null || request.Stars < 1 || request.Stars > 5)
        {
            HttpHelper.SendJsonResponse(context.Response, 400, "Invalid rating data (Stars 1-5 required)");
            return;
        }

        var rating = _ratingService.CreateRating(user.Id, mediaId, request.Stars, request.Comment);
        if (rating == null)
        {
            HttpHelper.SendJsonResponse(context.Response, 400, "Rating creation failed (User might have already rated this media)");
            return;
        }

        HttpHelper.SendJsonResponse(context.Response, 201, rating);
    }

    private async Task GetUserRatings(HttpListenerContext context)
    {
        var path = context.Request.Url?.AbsolutePath ?? "";
        var parameters = HttpHelper.ExtractPathParameters("/api/users/{userId}/ratings", path);

        if (!parameters.TryGetValue("userId", out var idString) || !int.TryParse(idString, out var userId))
        {
            HttpHelper.SendJsonResponse(context.Response, 400, "Invalid user ID");
            return;
        }

        var ratings = _ratingService.GetUserRatings(userId);
        HttpHelper.SendJsonResponse(context.Response, 200, ratings);
    }

    private async Task UpdateRating(HttpListenerContext context)
    {
        var user = AuthorizationHelper.AuthorizeRequest(context, _userService);
        if (user == null) return;

        var path = context.Request.Url?.AbsolutePath ?? "";
        var parameters = HttpHelper.ExtractPathParameters("/api/ratings/{id}", path);

        if (!parameters.TryGetValue("id", out var idString) || !int.TryParse(idString, out var ratingId))
        {
            HttpHelper.SendJsonResponse(context.Response, 400, "Invalid rating ID");
            return;
        }

        var request = await HttpHelper.ReadJsonBody<UpdateRatingRequest>(context.Request);
        if (request == null || request.Stars < 1 || request.Stars > 5)
        {
            HttpHelper.SendJsonResponse(context.Response, 400, "Invalid rating data");
            return;
        }

        var rating = _ratingService.UpdateRating(ratingId, user.Id, request.Stars, request.Comment);
        if (rating == null)
        {
            HttpHelper.SendJsonResponse(context.Response, 403, "Update failed (Not authorized or rating not found)");
            return;
        }

        HttpHelper.SendJsonResponse(context.Response, 200, rating);
    }

    private async Task DeleteRating(HttpListenerContext context)
    {
        var user = AuthorizationHelper.AuthorizeRequest(context, _userService);
        if (user == null) return;

        var path = context.Request.Url?.AbsolutePath ?? "";
        var parameters = HttpHelper.ExtractPathParameters("/api/ratings/{id}", path);

        if (!parameters.TryGetValue("id", out var idString) || !int.TryParse(idString, out var ratingId))
        {
            HttpHelper.SendJsonResponse(context.Response, 400, "Invalid rating ID");
            return;
        }

        var success = _ratingService.DeleteRating(ratingId, user.Id);
        if (!success)
        {
            HttpHelper.SendJsonResponse(context.Response, 403, "Delete failed (Not authorized or rating not found)");
            return;
        }

        HttpHelper.SendJsonResponse(context.Response, 200, "Rating deleted");
    }

    private async Task ConfirmRating(HttpListenerContext context)
    {
        var user = AuthorizationHelper.AuthorizeRequest(context, _userService);
        if (user == null) return;

        var path = context.Request.Url?.AbsolutePath ?? "";
        var parameters = HttpHelper.ExtractPathParameters("/api/ratings/{id}/confirm", path);

        if (!parameters.TryGetValue("id", out var idString) || !int.TryParse(idString, out var ratingId))
        {
            HttpHelper.SendJsonResponse(context.Response, 400, "Invalid rating ID");
            return;
        }
        
        var success = _ratingService.ConfirmRating(ratingId, user.Id);
        if (!success)
        {
            HttpHelper.SendJsonResponse(context.Response, 403, "Confirmation failed");
            return;
        }
        
        HttpHelper.SendJsonResponse(context.Response, 200, "Comment confirmed");
    }
}

public class CreateRatingRequest
{
    public int Stars { get; set; }
    public string Comment { get; set; } = "";
}

public class UpdateRatingRequest
{
    public int Stars { get; set; }
    public string Comment { get; set; } = "";
}
