using NUnit.Framework;
using Moq;
using MediaRatingsPlatform.Services;
using MediaRatingsPlatform.Interfaces;
using MediaRatingsPlatform.Models;

namespace MediaRatingsPlatform.Tests.Services;

[TestFixture]
public class FavoriteServiceTests
{
    private Mock<IFavoriteRepository> _favoriteRepositoryMock;
    private Mock<IMediaRepository> _mediaRepositoryMock;
    private FavoriteService _favoriteService;

    [SetUp]
    public void Setup()
    {
        _favoriteRepositoryMock = new Mock<IFavoriteRepository>();
        _mediaRepositoryMock = new Mock<IMediaRepository>();
        _favoriteService = new FavoriteService(_favoriteRepositoryMock.Object, _mediaRepositoryMock.Object);
    }

    [Test]
    public void AddToFavorites_ValidMedia_ReturnsTrue()
    {
        int userId = 1;
        int mediaId = 1;
        _mediaRepositoryMock.Setup(repo => repo.GetById(mediaId)).Returns(new MediaEntry { Id = mediaId, Description = "" });
        _favoriteRepositoryMock.Setup(repo => repo.IsFavorite(userId, mediaId)).Returns(false);
        _favoriteRepositoryMock.Setup(repo => repo.Create(It.IsAny<UserFavorite>())).Returns(new UserFavorite());

        var result = _favoriteService.AddToFavorites(userId, mediaId);

        Assert.That(result, Is.True);
    }

    [Test]
    public void AddToFavorites_AlreadyFavorite_ReturnsFalse()
    {
        int userId = 1;
        int mediaId = 1;
        _mediaRepositoryMock.Setup(repo => repo.GetById(mediaId)).Returns(new MediaEntry { Id = mediaId, Description = "" });
        _favoriteRepositoryMock.Setup(repo => repo.IsFavorite(userId, mediaId)).Returns(true);

        var result = _favoriteService.AddToFavorites(userId, mediaId);

        Assert.That(result, Is.False);
    }

    [Test]
    public void AddToFavorites_MediaNotFound_ReturnsFalse()
    {
        int userId = 1;
        int mediaId = 999;
        _mediaRepositoryMock.Setup(repo => repo.GetById(mediaId)).Returns((MediaEntry?)null);

        var result = _favoriteService.AddToFavorites(userId, mediaId);

        Assert.That(result, Is.False);
    }

    [Test]
    public void RemoveFromFavorites_Exists_ReturnsTrue()
    {
        int userId = 1;
        int mediaId = 1;
        _favoriteRepositoryMock.Setup(repo => repo.Delete(userId, mediaId)).Returns(true);

        var result = _favoriteService.RemoveFromFavorites(userId, mediaId);

        Assert.That(result, Is.True);
    }

    [Test]
    public void RemoveFromFavorites_NotExists_ReturnsFalse()
    {
        int userId = 1;
        int mediaId = 999;
        _favoriteRepositoryMock.Setup(repo => repo.Delete(userId, mediaId)).Returns(false);

        var result = _favoriteService.RemoveFromFavorites(userId, mediaId);

        Assert.That(result, Is.False);
    }

    [Test]
    public void GetUserFavorites_ReturnsMediaList()
    {
        int userId = 1;
        var favorites = new List<UserFavorite>
        {
            new UserFavorite { UserId = userId, MediaId = 1 },
            new UserFavorite { UserId = userId, MediaId = 2 }
        };
        _favoriteRepositoryMock.Setup(repo => repo.GetByUserId(userId)).Returns(favorites);
        _mediaRepositoryMock.Setup(repo => repo.GetById(1)).Returns(new MediaEntry { Id = 1, Title = "Movie 1", Description = "" });
        _mediaRepositoryMock.Setup(repo => repo.GetById(2)).Returns(new MediaEntry { Id = 2, Title = "Movie 2", Description = "" });

        var result = _favoriteService.GetUserFavorites(userId);

        Assert.That(result.Count(), Is.EqualTo(2));
    }

    [Test]
    public void IsUserFavorite_WhenTrue_ReturnsTrue()
    {
        int userId = 1;
        int mediaId = 1;
        _favoriteRepositoryMock.Setup(repo => repo.IsFavorite(userId, mediaId)).Returns(true);

        var result = _favoriteService.IsUserFavorite(userId, mediaId);

        Assert.That(result, Is.True);
    }
}
