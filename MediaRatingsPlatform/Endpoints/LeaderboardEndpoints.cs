using System.Net;
using MediaRatingsPlatform.Helpers;
using MediaRatingsPlatform.Interfaces;

namespace MediaRatingsPlatform.Endpoints;

public class LeaderboardEndpoints : IHttpEndpoint
{
    private readonly ILeaderboardService _leaderboardService;

    public LeaderboardEndpoints(ILeaderboardService leaderboardService)
    {
        _leaderboardService = leaderboardService;
    }

    public void RegisterRoutes(Router router)
    {
        router.AddRoute("GET", "/api/leaderboard", GetLeaderboard);
    }

    private async Task GetLeaderboard(HttpListenerContext context)
    {
        // Public endpoint
        var topUsers = _leaderboardService.GetMostActiveUsers(10); // Top 10
        HttpHelper.SendJsonResponse(context.Response, 200, topUsers);
    }
}
