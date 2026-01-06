using MediaRatingsPlatform.Interfaces;
using MediaRatingsPlatform.Models;

namespace MediaRatingsPlatform.Services;

public class FavoriteService : IFavoriteService
{
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IMediaRepository _mediaRepository;

    public FavoriteService(IFavoriteRepository favoriteRepository, IMediaRepository mediaRepository)
    {
        _favoriteRepository = favoriteRepository;
        _mediaRepository = mediaRepository;
    }

    public IEnumerable<MediaEntry> GetUserFavorites(int userId)
    {
        var favorites = _favoriteRepository.GetByUserId(userId);
        var mediaList = new List<MediaEntry>();

        foreach(var fav in favorites)
        {
            var media = _mediaRepository.GetById(fav.MediaId);
            if (media != null)
            {
                mediaList.Add(media);
            }
        }
        return mediaList;
    }

    public bool AddToFavorites(int userId, int mediaId)
    {
        if (_mediaRepository.GetById(mediaId) == null) return false;
        if (_favoriteRepository.IsFavorite(userId, mediaId)) return false;

        var fav = new UserFavorite
        {
            UserId = userId,
            MediaId = mediaId,
            CreatedAt = DateTime.UtcNow
        };

        _favoriteRepository.Create(fav);
        return true;
    }

    public bool RemoveFromFavorites(int userId, int mediaId)
    {
        return _favoriteRepository.Delete(userId, mediaId);
    }

    public bool IsUserFavorite(int userId, int mediaId)
    {
        return _favoriteRepository.IsFavorite(userId, mediaId);
    }

    public int GetFavoriteCount(int mediaId)
    {
        return _favoriteRepository.GetByMediaId(mediaId).Count();
    }
}
