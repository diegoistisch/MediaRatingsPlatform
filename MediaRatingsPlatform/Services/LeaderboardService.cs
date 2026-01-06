using MediaRatingsPlatform.Interfaces;

namespace MediaRatingsPlatform.Services;

public class LeaderboardService : ILeaderboardService
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IUserRepository _userRepository;

    public LeaderboardService(IRatingRepository ratingRepository, IUserRepository userRepository)
    {
        _ratingRepository = ratingRepository;
        _userRepository = userRepository;
    }

    public Dictionary<string, int> GetMostActiveUsers(int limit)
    {
        var topUsers = _ratingRepository.GetTopActiveUsers(limit);
        var result = new Dictionary<string, int>();

        foreach (var entry in topUsers)
        {
            var user = _userRepository.GetById(entry.Key);
            if (user != null)
            {
                result.Add(user.Username, entry.Value);
            }
            else 
            {
               // Fallback if user deleted?
               result.Add($"User {entry.Key}", entry.Value);
            }
        }
        return result;
    }
}
