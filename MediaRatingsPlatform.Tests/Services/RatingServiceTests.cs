using NUnit.Framework;
using Moq;
using MediaRatingsPlatform.Services;
using MediaRatingsPlatform.Interfaces;
using MediaRatingsPlatform.Models;

namespace MediaRatingsPlatform.Tests.Services;

[TestFixture]
public class RatingServiceTests
{
    private Mock<IRatingRepository> _ratingRepositoryMock;
    private Mock<IMediaRepository> _mediaRepositoryMock;
    private RatingService _ratingService;

    [SetUp]
    public void Setup()
    {
        _ratingRepositoryMock = new Mock<IRatingRepository>();
        _mediaRepositoryMock = new Mock<IMediaRepository>();
        _ratingService = new RatingService(_ratingRepositoryMock.Object, _mediaRepositoryMock.Object);
    }

    [Test]
    public void CreateRating_ValidInput_ReturnsRating()
    {
        int userId = 1;
        int mediaId = 1;
        int stars = 5;
        
        _ratingRepositoryMock.Setup(repo => repo.UserHasRated(userId, mediaId)).Returns(false);
        _ratingRepositoryMock.Setup(repo => repo.Create(It.IsAny<Rating>())).Returns((Rating r) => r);
        _mediaRepositoryMock.Setup(repo => repo.GetById(mediaId)).Returns(new MediaEntry { Id = mediaId, Description = "" });

        var result = _ratingService.CreateRating(userId, mediaId, stars, "Great!");

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Stars, Is.EqualTo(stars));
        Assert.That(result.IsCommentConfirmed, Is.False);
    }

    [Test]
    public void CreateRating_InvalidStars_ReturnsNull()
    {
        var result = _ratingService.CreateRating(1, 1, 6, "Overflow");

        Assert.That(result, Is.Null);
    }

    [Test]
    public void CreateRating_AlreadyRated_ReturnsNull()
    {
        _ratingRepositoryMock.Setup(repo => repo.UserHasRated(1, 1)).Returns(true);

        var result = _ratingService.CreateRating(1, 1, 5, "Again");

        Assert.That(result, Is.Null);
    }

    [Test]
    public void UpdateRating_ChangesComment_ResetsConfirmation()
    {
        int ratingId = 1;
        var existingRating = new Rating { Id = ratingId, UserId = 1, IsCommentConfirmed = true, Comment = "Old" };
        
        _ratingRepositoryMock.Setup(repo => repo.GetById(ratingId)).Returns(existingRating);
        _ratingRepositoryMock.Setup(repo => repo.Update(It.IsAny<Rating>())).Returns((Rating r) => r);
        _mediaRepositoryMock.Setup(repo => repo.GetById(It.IsAny<int>())).Returns(new MediaEntry { Description = "" });

        var result = _ratingService.UpdateRating(ratingId, 1, 5, "New Comment");

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Comment, Is.EqualTo("New Comment"));
        Assert.That(result.IsCommentConfirmed, Is.False);
    }

    [Test]
    public void ConfirmRating_ByOwner_SetsConfirmed()
    {
        int ratingId = 1;
        int userId = 1;
        var rating = new Rating { Id = ratingId, UserId = userId, IsCommentConfirmed = false };
        
        _ratingRepositoryMock.Setup(repo => repo.GetById(ratingId)).Returns(rating);
        _ratingRepositoryMock.Setup(repo => repo.Update(It.IsAny<Rating>())).Returns(rating);

        var result = _ratingService.ConfirmRating(ratingId, userId);

        Assert.That(result, Is.True);
        Assert.That(rating.IsCommentConfirmed, Is.True);
    }

    [Test]
    public void CalculateAverageRating_ReturnsCorrectAverage()
    {
        int mediaId = 1;
        _ratingRepositoryMock.Setup(repo => repo.CalculateAverageRating(mediaId)).Returns(4.0);

        var result = _ratingService.CalculateAverageRating(mediaId);

        Assert.That(result, Is.EqualTo(4.0));
    }

    [Test]
    public void UpdateMediaAverageRating_CalledOnCreate()
    {
        int mediaId = 1;
        _ratingRepositoryMock.Setup(repo => repo.Create(It.IsAny<Rating>())).Returns(new Rating());
        _ratingRepositoryMock.Setup(repo => repo.CalculateAverageRating(mediaId)).Returns(5.0);
        _mediaRepositoryMock.Setup(repo => repo.GetById(mediaId)).Returns(new MediaEntry { Id = mediaId, Description = "" });

        _ratingService.CreateRating(1, mediaId, 5, "Test");

        _mediaRepositoryMock.Verify(repo => repo.Update(It.IsAny<MediaEntry>()), Times.Once);
    }
}
