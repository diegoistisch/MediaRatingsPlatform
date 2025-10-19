namespace MediaRatingsPlatform.Models;

public class UserFavorite
{
    public int UserId { get; set; }
    public int MediaId  { get; set; }
    public DateTime CreatedAt  { get; set; }
}