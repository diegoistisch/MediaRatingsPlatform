using MediaRatingsPlatform.Interfaces;

namespace MediaRatingsPlatform.Services;

public class StatisticsService : IStatisticsService
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IMediaRepository _mediaRepository;

    public StatisticsService(IRatingRepository ratingRepository, IFavoriteRepository favoriteRepository, IMediaRepository mediaRepository)
    {
        _ratingRepository = ratingRepository;
        _favoriteRepository = favoriteRepository;
        _mediaRepository = mediaRepository;
    }

    public UserStatistics GetUserStatistics(int userId)
    {
        var ratings = _ratingRepository.GetByUserId(userId).ToList();
        var favorites = _favoriteRepository.GetByUserId(userId);

        var stats = new UserStatistics
        {
            TotalRatings = ratings.Count,
            FavoritesCount = favorites.Count()
        };

        if (stats.TotalRatings > 0)
        {
            stats.AverageScore = ratings.Average(r => r.Stars);
            
            // Calculate favorite genre
            var genreCounts = new Dictionary<string, int>();
            foreach (var r in ratings)
            {
                var media = _mediaRepository.GetById(r.MediaId);
                if (media != null && media.Genres != null)
                {
                    foreach (var genre in media.Genres)
                    {
                        if (genreCounts.ContainsKey(genre)) genreCounts[genre]++;
                        else genreCounts[genre] = 1;
                    }
                }
            }

            if (genreCounts.Any())
            {
                stats.FavoriteGenre = genreCounts.OrderByDescending(g => g.Value).First().Key;
            }
        }

        return stats;
    }
}
