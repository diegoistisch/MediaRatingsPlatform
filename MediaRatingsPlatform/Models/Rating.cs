namespace MediaRatingsPlatform.Models;

public class Rating
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int MediaId { get; set; }
    public int Stars { get; set; }
    public string Comment { get; set; }
    public bool IsCommentConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
}