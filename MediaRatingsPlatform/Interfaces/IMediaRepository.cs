using MediaRatingsPlatform.Models;
namespace MediaRatingsPlatform.Interfaces;

public interface IMediaRepository
{
    MediaEntry? GetById(int id);
    IEnumerable<MediaEntry> GetAll();
    IEnumerable<MediaEntry> GetByCreatorId(int creatorId);
    IEnumerable<MediaEntry> SearchByTitle(string title);
    IEnumerable<MediaEntry> FilterByGenre(string genre);
    IEnumerable<MediaEntry> FilterByType(Enums.MediaType type);
    IEnumerable<MediaEntry> FilterByYear(int year);
    IEnumerable<MediaEntry> FilterByAgeRestriction(Enums.AgeRestriction ageRestriction);
    MediaEntry Create(MediaEntry mediaEntry);
    MediaEntry? Update(MediaEntry mediaEntry);
    bool Delete(int id);
    bool Exists(int id);
}