using MediaRatingsPlatform.Interfaces;
using MediaRatingsPlatform.Models;

namespace MediaRatingsPlatform.Services;

public class LikeService : ILikeService
{
    private readonly ILikeRepository _likeRepository;
    private readonly IRatingRepository _ratingRepository;

    public LikeService(ILikeRepository likeRepository, IRatingRepository ratingRepository)
    {
        _likeRepository = likeRepository;
        _ratingRepository = ratingRepository;
    }

    public bool LikeRating(int userId, int ratingId)
    {
        var rating = _ratingRepository.GetById(ratingId);
        if (rating == null) return false;
        
        // Prevent liking own rating
        if (rating.UserId == userId) return false;

        if (_likeRepository.HasUserLikedRating(userId, ratingId)) return false;

        var like = new UserLike
        {
            UserId = userId,
            RatingId = ratingId,
            CreatedAt = DateTime.UtcNow
        };

        _likeRepository.Create(like);
        return true;
    }

    public bool UnlikeRating(int userId, int ratingId)
    {
        return _likeRepository.Delete(userId, ratingId);
    }

    public bool HasUserLikedRating(int userId, int ratingId)
    {
        return _likeRepository.HasUserLikedRating(userId, ratingId);
    }

    public int GetLikeCount(int ratingId)
    {
        return _likeRepository.GetLikeCount(ratingId);
    }

    public IEnumerable<UserLike> GetUserLikes(int userId)
    {
        return _likeRepository.GetByUserId(userId);
    }

    public bool UserCanLikeRating(int userId, int ratingId)
    {
         var rating = _ratingRepository.GetById(ratingId);
         if (rating == null) return false;
         return rating.UserId != userId;
    }
}
