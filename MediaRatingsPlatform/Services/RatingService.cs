using MediaRatingsPlatform.Interfaces;
using MediaRatingsPlatform.Models;

namespace MediaRatingsPlatform.Services;

public class RatingService : IRatingService
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IMediaRepository _mediaRepository;

    public RatingService(IRatingRepository ratingRepository, IMediaRepository mediaRepository)
    {
        _ratingRepository = ratingRepository;
        _mediaRepository = mediaRepository;
    }

    public Rating? GetRating(int id)
    {
        return _ratingRepository.GetById(id);
    }

    public IEnumerable<Rating> GetMediaRatings(int mediaId)
    {
        return _ratingRepository.GetByMediaId(mediaId).Where(r => r.IsCommentConfirmed || string.IsNullOrEmpty(r.Comment));
    }
    
    public IEnumerable<Rating> GetUserRatings(int userId)
    {
        return _ratingRepository.GetByUserId(userId);
    }

    public Rating? CreateRating(int userId, int mediaId, int stars, string? comment)
    {
        if (stars < 1 || stars > 5) return null;
        if (_ratingRepository.UserHasRated(userId, mediaId)) return null;

        var rating = new Rating
        {
            UserId = userId,
            MediaId = mediaId,
            Stars = stars,
            Comment = comment ?? "",
            IsCommentConfirmed = false // Default to false
        };

        var created = _ratingRepository.Create(rating);
        UpdateMediaAverageRating(mediaId);
        return created;
    }

    public Rating? UpdateRating(int ratingId, int userId, int stars, string? comment)
    {
        var rating = _ratingRepository.GetById(ratingId);
        if (rating == null || rating.UserId != userId) return null;
        
        if (stars < 1 || stars > 5) return null;

        rating.Stars = stars;
        rating.Comment = comment ?? "";
        rating.IsCommentConfirmed = false; 

        var updated = _ratingRepository.Update(rating);
        if (updated != null)
        {
            UpdateMediaAverageRating(rating.MediaId);
        }
        return updated;
    }

    public bool DeleteRating(int ratingId, int userId)
    {
        var rating = _ratingRepository.GetById(ratingId);
        if (rating == null || rating.UserId != userId) return false;

        var success = _ratingRepository.Delete(ratingId);
        if (success)
        {
            UpdateMediaAverageRating(rating.MediaId);
        }
        return success;
    }

    public bool ConfirmRating(int ratingId, int userId)
    {
        var rating = _ratingRepository.GetById(ratingId);
        if (rating == null || rating.UserId != userId) return false;

        rating.IsCommentConfirmed = true;
        return _ratingRepository.Update(rating) != null;
    }

    public double CalculateAverageRating(int mediaId)
    {
        return _ratingRepository.CalculateAverageRating(mediaId);
    }
    
    public bool UserCanModifyRating(int ratingId, int userId)
    {
        var rating = _ratingRepository.GetById(ratingId);
        return rating != null && rating.UserId == userId;
    }

    private void UpdateMediaAverageRating(int mediaId)
    {
        var avg = CalculateAverageRating(mediaId);
        var media = _mediaRepository.GetById(mediaId);
        if (media != null)
        {
            media.AverageRating = avg;
            _mediaRepository.Update(media);
        }
    }
}
