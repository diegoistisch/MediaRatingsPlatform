using System.Net;
using MediaRatingsPlatform.Helpers;
using MediaRatingsPlatform.Interfaces;

namespace MediaRatingsPlatform.Endpoints;

public class RecommendationEndpoints : IHttpEndpoint
{
    private readonly IRecommendationService _recommendationService;
    private readonly IUserService _userService;

    public RecommendationEndpoints(IRecommendationService recommendationService, IUserService userService)
    {
        _recommendationService = recommendationService;
        _userService = userService;
    }

    public void RegisterRoutes(Router router)
    {
        router.AddRoute("GET", "/api/users/{userId}/recommendations", GetRecommendations);
    }

    private async Task GetRecommendations(HttpListenerContext context)
    {
        var user = AuthorizationHelper.AuthorizeRequest(context, _userService);
        if (user == null) return;

        var path = context.Request.Url?.AbsolutePath ?? "";
        var parameters = HttpHelper.ExtractPathParameters("/api/users/{userId}/recommendations", path);

        if (!parameters.TryGetValue("userId", out var idString) || !int.TryParse(idString, out var userId))
        {
            HttpHelper.SendJsonResponse(context.Response, 400, "Invalid user ID");
            return;
        }

        if (user.Id != userId)
        {
            HttpHelper.SendJsonResponse(context.Response, 403, "Access denied");
            return;
        }

        var recommendations = _recommendationService.GetRecommendations(userId, 5); // Top 5
        HttpHelper.SendJsonResponse(context.Response, 200, recommendations);
    }
}
