namespace MediaRatingsPlatform;

public class MediaEntry
{
    public int Id { get; set; }
    public int CreatorId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public Enums.MediaType Type { get; set; }
    public Enums.AgeRestriction AgeRestriction { get; set; }
    public int ReleaseYear { get; set; }
    public List<string> Genres { get; set; }
    public List<Rating> RatingsList { get; set; }
    public double AverageRating { get; set; }
    public DateTime CreatedAt { get; set; }

    public MediaEntry()
    {
        
    }
}