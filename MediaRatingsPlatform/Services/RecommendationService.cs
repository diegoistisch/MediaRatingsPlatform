using MediaRatingsPlatform.Interfaces;
using MediaRatingsPlatform.Models;

namespace MediaRatingsPlatform.Services;

public class RecommendationService : IRecommendationService
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IMediaRepository _mediaRepository;

    public RecommendationService(IRatingRepository ratingRepository, IMediaRepository mediaRepository)
    {
        _ratingRepository = ratingRepository;
        _mediaRepository = mediaRepository;
    }

    public IEnumerable<MediaEntry> GetRecommendations(int userId, int limit)
    {
        // 1. Get users highest ratings 
        var userRatings = _ratingRepository.GetByUserId(userId);
        var likedMediaIds = userRatings.Where(r => r.Stars >= 4).Select(r => r.MediaId).ToHashSet();
        
        if (!likedMediaIds.Any()) return new List<MediaEntry>();

        var likedMedia = new List<MediaEntry>();
        foreach (var id in likedMediaIds)
        {
            var m = _mediaRepository.GetById(id);
            if (m != null) likedMedia.Add(m);
        }

        // 2. Extract preferred genres
        var preferredGenres = likedMedia
            .SelectMany(m => m.Genres)
            .GroupBy(g => g)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .Take(3) // Top 3 genres
            .ToList();

        if (!preferredGenres.Any()) return new List<MediaEntry>();

        // 3. Find other media with these genres that user hasnt rated
        var allMedia = _mediaRepository.GetAll(); // Or use Search/Filter if available
        var ratedMediaIds = userRatings.Select(r => r.MediaId).ToHashSet();

        var recommendations = allMedia
            .Where(m => !ratedMediaIds.Contains(m.Id)) // not seen yet
            .Select(m => new 
            { 
                Media = m, 
                Score = CalculateRelevanceScore(m, preferredGenres) 
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Media.AverageRating)
            .Take(limit)
            .Select(x => x.Media)
            .ToList();

        return recommendations;
    }

    private int CalculateRelevanceScore(MediaEntry media, List<string> preferredGenres)
    {
        int score = 0;
        foreach (var genre in media.Genres)
        {
            if (preferredGenres.Contains(genre)) score++;
        }
        return score;
    }
}
