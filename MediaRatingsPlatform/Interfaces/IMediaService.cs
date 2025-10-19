namespace MediaRatingsPlatform.Interfaces;

public interface IMediaService
{
    MediaEntry? GetMediaById(int id);
    IEnumerable<MediaEntry> GetAllMedia();
    IEnumerable<MediaEntry> GetUserMedia(int userId);
    IEnumerable<MediaEntry> SearchMedia(string title);
    IEnumerable<MediaEntry> FilterMedia(string? genre, Enums.MediaType? type, int? year, Enums.AgeRestriction? ageRestriction);
    IEnumerable<MediaEntry> SortMedia(IEnumerable<MediaEntry> media, string sortBy);
    MediaEntry? CreateMedia(int userId, string title, string description, Enums.MediaType type, int releaseYear, List<string> genres, Enums.AgeRestriction ageRestriction);
    MediaEntry? UpdateMedia(int mediaId, int userId, string title, string description, Enums.MediaType type, int releaseYear, List<string> genres, Enums.AgeRestriction ageRestriction);
    bool DeleteMedia(int mediaId, int userId);
    bool UserCanModifyMedia(int mediaId, int userId);
}