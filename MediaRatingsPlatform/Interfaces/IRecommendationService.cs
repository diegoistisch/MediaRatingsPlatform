using MediaRatingsPlatform.Models;

namespace MediaRatingsPlatform.Interfaces;

public interface IRecommendationService
{
    IEnumerable<MediaEntry> GetRecommendations(int userId, int limit);
}
