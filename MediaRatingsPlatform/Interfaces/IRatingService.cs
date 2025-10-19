using MediaRatingsPlatform.Models;
namespace MediaRatingsPlatform;

public interface IRatingService
{
    Rating? GetRating(int id);
    IEnumerable<Rating> GetMediaRatings(int mediaId);
    IEnumerable<Rating> GetUserRatings(int userId);
    Rating? CreateRating(int userId, int mediaId, int stars, string? comment);
    Rating? UpdateRating(int ratingId, int userId, int stars, string? comment);
    bool DeleteRating(int ratingId, int userId);
    bool ConfirmRating(int ratingId, int userId);
    double CalculateAverageRating(int mediaId);
    bool UserCanModifyRating(int ratingId, int userId);
}