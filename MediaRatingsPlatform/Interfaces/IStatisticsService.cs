using MediaRatingsPlatform.Models;

namespace MediaRatingsPlatform.Interfaces;

public interface IStatisticsService
{
    UserStatistics GetUserStatistics(int userId);
}

public class UserStatistics
{
    public int TotalRatings { get; set; }
    public double AverageScore { get; set; }
    public string FavoriteGenre { get; set; } = "";
    public int FavoritesCount { get; set; }
}
