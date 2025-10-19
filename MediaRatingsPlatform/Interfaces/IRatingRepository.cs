using MediaRatingsPlatform.Models;
namespace MediaRatingsPlatform;

public interface IRatingRepository
{
    Rating? GetById(int id);
    IEnumerable<Rating> GetByMediaId(int mediaId);
    IEnumerable<Rating> GetByUserId(int userId);
    Rating? GetByUserAndMedia(int userId, int mediaId);
    Rating Create(Rating rating);
    Rating? Update(Rating rating);
    bool Delete(int id);
    bool Exists(int id);
    bool UserHasRated(int userId, int mediaId);
}