namespace MediaRatingsPlatform.Models;

public class UserLike
{
    public int UserId { get; set; }
    public int RatingId  { get; set; }
    public DateTime CreatedAt  { get; set; }
}