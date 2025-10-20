using MediaRatingsPlatform.Interfaces;
using MediaRatingsPlatform.Models;

namespace MediaRatingsPlatform.Services;

public class MediaService : IMediaService
{
    private readonly IMediaRepository _mediaRepository;

    public MediaService(IMediaRepository mediaRepository)
    {
        _mediaRepository = mediaRepository;
    }

    public MediaEntry? CreateMedia(int userId, string title, string description, Enums.MediaType type, int releaseYear, List<string> genres, Enums.AgeRestriction ageRestriction)
    {
        if (string.IsNullOrWhiteSpace(title))
            return null;

        if (releaseYear < 1800 || releaseYear > DateTime.UtcNow.Year + 5)
            return null;

        var mediaEntry = new MediaEntry
        {
            CreatorId = userId,
            Title = title,
            Description = description ?? "",
            Type = type,
            ReleaseYear = releaseYear,
            Genres = genres ?? new List<string>(),
            AgeRestriction = ageRestriction
        };

        return _mediaRepository.Create(mediaEntry);
    }

    public MediaEntry? GetMediaById(int id)
    {
        return _mediaRepository.GetById(id);
    }

    public IEnumerable<MediaEntry> GetAllMedia()
    {
        return _mediaRepository.GetAll();
    }

    public IEnumerable<MediaEntry> GetUserMedia(int userId)
    {
        return _mediaRepository.GetByCreatorId(userId);
    }

    public IEnumerable<MediaEntry> SearchMedia(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return new List<MediaEntry>();

        return _mediaRepository.SearchByTitle(title);
    }

    public IEnumerable<MediaEntry> FilterMedia(string? genre, Enums.MediaType? type, int? year, Enums.AgeRestriction? ageRestriction)
    {
        var media = _mediaRepository.GetAll();

        if (!string.IsNullOrWhiteSpace(genre))
            media = _mediaRepository.FilterByGenre(genre);

        if (type.HasValue)
            media = media.Where(m => m.Type == type.Value);

        if (year.HasValue)
            media = media.Where(m => m.ReleaseYear == year.Value);

        if (ageRestriction.HasValue)
            media = media.Where(m => m.AgeRestriction == ageRestriction.Value);

        return media;
    }

    public IEnumerable<MediaEntry> SortMedia(IEnumerable<MediaEntry> media, string sortBy)
    {
        return sortBy?.ToLower() switch
        {
            "title" => media.OrderBy(m => m.Title),
            "year" => media.OrderByDescending(m => m.ReleaseYear),
            "rating" => media.OrderByDescending(m => m.AverageRating),
            "date" => media.OrderByDescending(m => m.CreatedAt),
            _ => media
        };
    }

    public MediaEntry? UpdateMedia(int mediaId, int userId, string title, string description, Enums.MediaType type, int releaseYear, List<string> genres, Enums.AgeRestriction ageRestriction)
    {
        if (!UserCanModifyMedia(mediaId, userId))
            return null;

        if (string.IsNullOrWhiteSpace(title))
            return null;

        if (releaseYear < 1800 || releaseYear > DateTime.UtcNow.Year + 5)
            return null;

        var mediaEntry = new MediaEntry
        {
            Id = mediaId,
            Title = title,
            Description = description ?? "",
            Type = type,
            ReleaseYear = releaseYear,
            Genres = genres ?? new List<string>(),
            AgeRestriction = ageRestriction
        };

        return _mediaRepository.Update(mediaEntry);
    }

    public bool DeleteMedia(int mediaId, int userId)
    {
        if (!UserCanModifyMedia(mediaId, userId))
            return false;

        return _mediaRepository.Delete(mediaId);
    }

    public bool UserCanModifyMedia(int mediaId, int userId)
    {
        var media = _mediaRepository.GetById(mediaId);
        if (media == null)
            return false;

        return media.CreatorId == userId;
    }
}