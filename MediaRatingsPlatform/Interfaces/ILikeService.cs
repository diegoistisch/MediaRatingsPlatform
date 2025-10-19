namespace MediaRatingsPlatform.Interfaces;

public interface ILikeService
{
    bool LikeRating(int userId, int ratingId);
    bool UnlikeRating(int userId, int ratingId);
    bool HasUserLikedRating(int userId, int ratingId);
    int GetLikeCount(int ratingId);
    IEnumerable<UserLike> GetUserLikes(int userId);
    bool UserCanLikeRating(int userId, int ratingId);
}