namespace MediaRatingsPlatform.Interfaces;

public interface IFavoriteService
{
    IEnumerable<MediaEntry> GetUserFavorites(int userId);
    bool AddToFavorites(int userId, int mediaId);
    bool RemoveFromFavorites(int userId, int mediaId);
    bool IsUserFavorite(int userId, int mediaId);
    int GetFavoriteCount(int mediaId);
}