using MediaRatingsPlatform.Interfaces;
using MediaRatingsPlatform.Models;

namespace MediaRatingsPlatform.Repositories;

public class MediaRepository : IMediaRepository
{
    private readonly List<MediaEntry> _mediaEntries = new List<MediaEntry>();
    private int _nextId = 1;

    public MediaEntry Create(MediaEntry mediaEntry)
    {
        mediaEntry.Id = _nextId++;
        mediaEntry.CreatedAt = DateTime.UtcNow;
        mediaEntry.Genres ??= new List<string>();
        mediaEntry.RatingsList ??= new List<Rating>();
        mediaEntry.AverageRating = 0;
        _mediaEntries.Add(mediaEntry);
        return mediaEntry;
    }

    public MediaEntry? GetById(int id)
    {
        return _mediaEntries.FirstOrDefault(m => m.Id == id);
    }

    public IEnumerable<MediaEntry> GetAll()
    {
        return _mediaEntries;
    }

    public IEnumerable<MediaEntry> GetByCreatorId(int creatorId)
    {
        return _mediaEntries.Where(m => m.CreatorId == creatorId);
    }

    public IEnumerable<MediaEntry> SearchByTitle(string title)
    {
        return _mediaEntries.Where(m => m.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<MediaEntry> FilterByGenre(string genre)
    {
        return _mediaEntries.Where(m => m.Genres.Contains(genre, StringComparer.OrdinalIgnoreCase));
    }

    public IEnumerable<MediaEntry> FilterByType(Enums.MediaType type)
    {
        return _mediaEntries.Where(m => m.Type == type);
    }

    public IEnumerable<MediaEntry> FilterByYear(int year)
    {
        return _mediaEntries.Where(m => m.ReleaseYear == year);
    }

    public IEnumerable<MediaEntry> FilterByAgeRestriction(Enums.AgeRestriction ageRestriction)
    {
        return _mediaEntries.Where(m => m.AgeRestriction == ageRestriction);
    }

    public MediaEntry? Update(MediaEntry mediaEntry)
    {
        var existing = GetById(mediaEntry.Id);
        if (existing == null)
            return null;

        existing.Title = mediaEntry.Title;
        existing.Description = mediaEntry.Description;
        existing.Type = mediaEntry.Type;
        existing.AgeRestriction = mediaEntry.AgeRestriction;
        existing.ReleaseYear = mediaEntry.ReleaseYear;
        existing.Genres = mediaEntry.Genres ?? new List<string>();

        return existing;
    }

    public bool Delete(int id)
    {
        var media = GetById(id);
        if (media == null)
            return false;

        _mediaEntries.Remove(media);
        return true;
    }

    public bool Exists(int id)
    {
        return _mediaEntries.Any(m => m.Id == id);
    }
}