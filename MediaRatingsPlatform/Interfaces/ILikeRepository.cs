namespace MediaRatingsPlatform.Interfaces;

public interface ILikeRepository
{
    UserLike? GetById(int id);
    IEnumerable<UserLike> GetByUserId(int userId);
    IEnumerable<UserLike> GetByRatingId(int ratingId);
    UserLike? GetByUserAndRating(int userId, int ratingId);
    UserLike Create(UserLike like);
    bool Delete(int userId, int ratingId);
    bool HasUserLikedRating(int userId, int ratingId);
    int GetLikeCount(int ratingId);
}