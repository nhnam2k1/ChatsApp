using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Microsoft.Extensions.Primitives;
using Auth0.ManagementApi.Paging;

[TestFixture]
public class UserServiceTests
{
    private Mock<HttpRequest> _mockRequest;
    private Mock<ILogger<UserService>> _mockLogger;
    private Mock<IManagementApiClient> _mockManagementApiClient;
    private UserService _userService;

    [SetUp]
    public void Setup()
    {
        _mockRequest = new Mock<HttpRequest>();
        _mockLogger = new Mock<ILogger<UserService>>();
        _mockManagementApiClient = new Mock<IManagementApiClient>();
        _userService = new UserService(_mockManagementApiClient.Object, _mockLogger.Object);
    }

    [Test]
    public async Task GetUsers_ReturnsBadRequest_WhenCurrentUserIdIsMissing()
    {
        // Arrange
        var query = new QueryCollection(new Dictionary<string, StringValues>());
        _mockRequest.Setup(r => r.Query).Returns(query);

        // Act
        var result = await _userService.GetUsers(_mockRequest.Object);
        
        var error = new { error = "current_user_id parameter is required" };
        var check = Results.BadRequest(error);

        // Assert
        Assert.IsNotNull(result);
        Assert.That(result.ToString(), Is.EqualTo(check.ToString()));
    }

    [Test]
    public async Task GetUsers_HandlesException()
    {
        // Arrange
        var query = new QueryCollection(new Dictionary<string, StringValues>
        {
            { "current_user_id", "user123" }
        });
        _mockRequest.Setup(r => r.Query).Returns(query);

        _mockManagementApiClient.Setup(m => m.Users.GetAllAsync(It.IsAny<GetUsersRequest>(), null, default))
            .ThrowsAsync(new Exception("Something went wrong"));

        // Act
        var result = await _userService.GetUsers(_mockRequest.Object);
        var validate = Results.StatusCode(500);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.That(result.ToString(), Is.EqualTo(validate.ToString()));
    }

    [Test]
    public async Task GetUsers_ReturnsFilteredUsers()
    {
        // Arrange
        var query = new QueryCollection(new Dictionary<string, StringValues>
        {
            { "current_user_id", "user123" }
        });
        _mockRequest.Setup(r => r.Query).Returns(query);

        var users = new PagedList<User>
        {
            new User { UserId = "user123", NickName = "John", Picture = "john.jpg" },
            new User { UserId = "user456", NickName = "Jane", Picture = "jane.jpg" }
        };
        _mockManagementApiClient.Setup(m => m.Users.GetAllAsync(It.IsAny<GetUsersRequest>(), null, default))
            .ReturnsAsync(users);

        // Act
        var result = await _userService.GetUsers(_mockRequest.Object);
        var check = Results.Ok(users[1]);

        // Assert
        Assert.IsNotNull(result);
        Assert.That(result.ToString(), Is.EqualTo(check.ToString()));
    }
}