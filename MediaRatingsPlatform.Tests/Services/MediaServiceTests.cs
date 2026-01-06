using NUnit.Framework;
using Moq;
using MediaRatingsPlatform.Services;
using MediaRatingsPlatform.Interfaces;
using MediaRatingsPlatform.Models;
using static MediaRatingsPlatform.Models.Enums;

namespace MediaRatingsPlatform.Tests.Services;

[TestFixture]
public class MediaServiceTests
{
    private Mock<IMediaRepository> _mediaRepositoryMock;
    private MediaService _mediaService;

    [SetUp]
    public void Setup()
    {
        _mediaRepositoryMock = new Mock<IMediaRepository>();
        _mediaService = new MediaService(_mediaRepositoryMock.Object);
    }

    [Test]
    public void CreateMedia_ValidInput_ReturnsMedia()
    {
        int creatorId = 1;
        string title = "Test Movie";
        _mediaRepositoryMock.Setup(repo => repo.Create(It.IsAny<MediaEntry>())).Returns((MediaEntry m) => m);

        var result = _mediaService.CreateMedia(creatorId, title, "Description", MediaType.Movie, 2024, new List<string> { "Action" }, AgeRestriction.AllAges);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo(title));
        Assert.That(result.CreatorId, Is.EqualTo(creatorId));
    }

    [Test]
    public void CreateMedia_EmptyTitle_ReturnsNull()
    {
        var result = _mediaService.CreateMedia(1, "", "Desc", MediaType.Movie, 2024, new List<string>(), AgeRestriction.AllAges);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void CreateMedia_InvalidYear_ReturnsNull()
    {
        var result = _mediaService.CreateMedia(1, "Title", "Desc", MediaType.Movie, 1800, new List<string>(), AgeRestriction.AllAges);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void UpdateMedia_ByOwner_ReturnsUpdated()
    {
        int mediaId = 1;
        int ownerId = 1;
        var existingMedia = new MediaEntry { Id = mediaId, CreatorId = ownerId, Title = "Old", Description = "" };
        
        _mediaRepositoryMock.Setup(repo => repo.GetById(mediaId)).Returns(existingMedia);
        _mediaRepositoryMock.Setup(repo => repo.Update(It.IsAny<MediaEntry>())).Returns((MediaEntry m) => m);

        var result = _mediaService.UpdateMedia(mediaId, ownerId, "New Title", "Desc", MediaType.Movie, 2024, new List<string>(), AgeRestriction.AllAges);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo("New Title"));
    }

    [Test]
    public void UpdateMedia_NotOwner_ReturnsNull()
    {
        int mediaId = 1;
        var existingMedia = new MediaEntry { Id = mediaId, CreatorId = 1, Title = "Test", Description = "" };
        
        _mediaRepositoryMock.Setup(repo => repo.GetById(mediaId)).Returns(existingMedia);

        var result = _mediaService.UpdateMedia(mediaId, 2, "Hacked", "Desc", MediaType.Movie, 2024, new List<string>(), AgeRestriction.AllAges);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void DeleteMedia_ByOwner_ReturnsTrue()
    {
        int mediaId = 1;
        int ownerId = 1;
        var existingMedia = new MediaEntry { Id = mediaId, CreatorId = ownerId, Title = "Test", Description = "" };
        
        _mediaRepositoryMock.Setup(repo => repo.GetById(mediaId)).Returns(existingMedia);
        _mediaRepositoryMock.Setup(repo => repo.Delete(mediaId)).Returns(true);

        var result = _mediaService.DeleteMedia(mediaId, ownerId);

        Assert.That(result, Is.True);
    }

    [Test]
    public void FilterMedia_ByGenre_ReturnsFiltered()
    {
        var actionMovies = new List<MediaEntry>
        {
            new MediaEntry { Id = 1, Title = "Action Movie", Genres = new List<string> { "Action" }, Description = "" }
        };
        _mediaRepositoryMock.Setup(repo => repo.GetAll()).Returns(new List<MediaEntry>());
        _mediaRepositoryMock.Setup(repo => repo.FilterByGenre("Action")).Returns(actionMovies);

        var result = _mediaService.FilterMedia("Action", null, null, null);

        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result.First().Title, Is.EqualTo("Action Movie"));
    }

    [Test]
    public void SortMedia_ByRating_ReturnsSorted()
    {
        var media = new List<MediaEntry>
        {
            new MediaEntry { Id = 1, Title = "Low Rated", AverageRating = 2.0, Description = "" },
            new MediaEntry { Id = 2, Title = "High Rated", AverageRating = 4.5, Description = "" }
        };

        var result = _mediaService.SortMedia(media, "rating").ToList();

        Assert.That(result[0].Title, Is.EqualTo("High Rated"));
        Assert.That(result[1].Title, Is.EqualTo("Low Rated"));
    }
}
