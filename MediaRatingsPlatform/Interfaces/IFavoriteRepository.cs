using MediaRatingsPlatform.Models;
namespace MediaRatingsPlatform.Interfaces;

public interface IFavoriteRepository
{
    UserFavorite? GetById(int id);
    IEnumerable<UserFavorite> GetByUserId(int userId);
    IEnumerable<UserFavorite> GetByMediaId(int mediaId);
    UserFavorite? GetByUserAndMedia(int userId, int mediaId);
    UserFavorite Create(UserFavorite favorite);
    bool Delete(int userId, int mediaId);
    bool IsFavorite(int userId, int mediaId);
}