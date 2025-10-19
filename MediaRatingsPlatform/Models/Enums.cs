namespace MediaRatingsPlatform.Models;

public class Enums
{
    public enum MediaType
    {
        Movie,
        Series,
        Game
    }

    public enum AgeRestriction
    {
        AllAges = 0,
        Teen = 13,
        Mature = 16,
        Adult = 18
    }
}