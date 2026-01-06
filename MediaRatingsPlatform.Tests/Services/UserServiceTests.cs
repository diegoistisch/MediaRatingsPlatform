using NUnit.Framework;
using Moq;
using MediaRatingsPlatform.Services;
using MediaRatingsPlatform.Interfaces;
using MediaRatingsPlatform.Models;

namespace MediaRatingsPlatform.Tests.Services;

[TestFixture]
public class UserServiceTests
{
    private Mock<IUserRepository> _userRepositoryMock;
    private UserService _userService;

    private static readonly string HashedValidPassword = BCrypt.Net.BCrypt.HashPassword("validpassword");
    private static readonly string HashedCorrectPassword = BCrypt.Net.BCrypt.HashPassword("correctpassword");
    private static readonly string HashedPassword = BCrypt.Net.BCrypt.HashPassword("password");

    [SetUp]
    public void Setup()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _userService = new UserService(_userRepositoryMock.Object);
    }

    [Test]
    public void RegisterUser_ValidInput_ReturnsUser()
    {
        string username = "testuser";
        string email = "test@example.com";
        string password = "password";
        
        _userRepositoryMock.Setup(repo => repo.GetByUsername(username)).Returns((User?)null);
        _userRepositoryMock.Setup(repo => repo.Add(It.IsAny<User>()));

        var result = _userService.RegisterUser(username, email, password);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Username, Is.EqualTo(username));
        Assert.That(result.Email, Is.EqualTo(email));
        Assert.That(result.Password, Is.Not.EqualTo(password));
        Assert.That(BCrypt.Net.BCrypt.Verify(password, result.Password), Is.True);
    }

    [Test]
    public void RegisterUser_DuplicateUsername_ReturnsNull()
    {
        string username = "existinguser";
        _userRepositoryMock.Setup(repo => repo.GetByUsername(username)).Returns(new User { Username = username, Email = "", Password = "" });

        var result = _userService.RegisterUser(username, "email", "password");

        Assert.That(result, Is.Null);
    }

    [Test]
    public void AuthenticateUser_ValidCredentials_ReturnsResult()
    {
        string username = "validuser";
        string password = "validpassword";
        int userId = 1;

        var user = new User { Id = userId, Username = username, Email = "test@test.com", Password = HashedValidPassword };
        _userRepositoryMock.Setup(repo => repo.GetByUsername(username)).Returns(user);

        var result = _userService.AuthenticateUser(username, password);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserId, Is.EqualTo(userId));
        Assert.That(result.Username, Is.EqualTo(username));
        Assert.That(string.IsNullOrEmpty(result.Token), Is.False);
    }

    [Test]
    public void AuthenticateUser_InvalidPassword_ReturnsNull()
    {
        string username = "validuser";
        string password = "wrongpassword";

        var user = new User { Id = 1, Username = username, Email = "test@test.com", Password = HashedCorrectPassword };
        _userRepositoryMock.Setup(repo => repo.GetByUsername(username)).Returns(user);

        var result = _userService.AuthenticateUser(username, password);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void AuthenticateUser_UserNotFound_ReturnsNull()
    {
        string username = "nonexistent";
        _userRepositoryMock.Setup(repo => repo.GetByUsername(username)).Returns((User?)null);

        var result = _userService.AuthenticateUser(username, "password");

        Assert.That(result, Is.Null);
    }

    [Test]
    public void ValidateToken_ValidToken_ReturnsTrue()
    {
        string username = "testuser";
        string password = "password";
        var user = new User { Id = 1, Username = username, Email = "email", Password = HashedPassword };
        _userRepositoryMock.Setup(repo => repo.GetByUsername(username)).Returns(user);
        
        var authResult = _userService.AuthenticateUser(username, password);
        Assert.That(authResult, Is.Not.Null);
        string token = authResult.Token;

        bool isValid = _userService.ValidateToken(token);

        Assert.That(isValid, Is.True);
    }
}
