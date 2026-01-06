using MediaRatingsPlatform.Models;

namespace MediaRatingsPlatform.Interfaces;

public interface ILeaderboardService
{
    Dictionary<string, int> GetMostActiveUsers(int limit);
}
