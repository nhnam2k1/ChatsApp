using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MongoDB.Driver;
using FileUploadApi.Controllers;

namespace ChatApp.Tests
{
    public class ChatHubControllerTests
    {
        private Mock<IMongoCollection<ChatMessage>> _mockChatMessages;
        private Mock<IMongoClient> _mockMongoClient;
        private Mock<IEncryptionHelper> _mockEncryptionHelper;
        private Mock<ILogger<ChatHubController>> _mockLogger;
        private ChatHubController _controller;

        [SetUp]
        public void Setup()
        {
            _mockChatMessages = new Mock<IMongoCollection<ChatMessage>>();
            _mockMongoClient = new Mock<IMongoClient>();
            _mockEncryptionHelper = new Mock<IEncryptionHelper>();
            _mockLogger = new Mock<ILogger<ChatHubController>>();

            var mockDatabase = new Mock<IMongoDatabase>();
            mockDatabase.Setup(db => db.GetCollection<ChatMessage>("ChatMessages", null))
                        .Returns(_mockChatMessages.Object);

            _mockMongoClient.Setup(client => client.GetDatabase("Chatsapp", null))
                            .Returns(mockDatabase.Object);

            _controller = new ChatHubController(
                _mockMongoClient.Object,
                _mockEncryptionHelper.Object,
                _mockLogger.Object
            );

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "testUserId")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Test]
        public async Task FetchMessages_UserIdNotFound_ReturnsUnauthorized()
        {
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();

            var result = await _controller.FetchMessages(new FetchMessagesRequest());

            Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
        }

        [Test]
        public async Task FetchMessages_InvalidRequestPayload_ReturnsBadRequest()
        {
            var result = await _controller.FetchMessages(It.IsAny<FetchMessagesRequest>());

            Assert.IsInstanceOf<BadRequestObjectResult>(result);

            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult?.Value, Is.EqualTo("Invalid request payload"));
        }

        [Test]
        public async Task FetchMessages_ExceptionThrown_ReturnsInternalServerError()
        {
            _mockChatMessages.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<ChatMessage>>(), It.IsAny<FindOptions<ChatMessage, ChatMessage>>(), default))
                             .ThrowsAsync(new Exception("Test exception"));

            var result = await _controller.FetchMessages(new FetchMessagesRequest
            {
                UserId = "user1",
                RecipientId = "user2"
            });

            Assert.IsInstanceOf<ObjectResult>(result);

            var objectResult = result as ObjectResult;
            Assert.That(objectResult?.StatusCode, Is.EqualTo(500));
        }

        [Test]
        public async Task FetchMessages_ValidRequest_ReturnsMessages()
        {
            string originalContent = "This is the original content";
            byte[] compressedBytes = CompressionHelper.Compress(originalContent);
            string base64Compressed = Convert.ToBase64String(compressedBytes);

            var messages = new List<ChatMessage>
            {
                new ChatMessage { UserId = "user1", RecipientId = "user2", Content = Convert.ToBase64String(new byte[0]), Timestamp = DateTime.UtcNow },
                new ChatMessage { UserId = "user2", RecipientId = "user1", Content = Convert.ToBase64String(new byte[0]), Timestamp = DateTime.UtcNow }
            };

            var mockCursor = new Mock<IAsyncCursor<ChatMessage>>();
            mockCursor.Setup(_ => _.Current).Returns(messages);
            mockCursor
                .SetupSequence(_ => _.MoveNext(It.IsAny<System.Threading.CancellationToken>()))
                .Returns(true)
                .Returns(false);
            mockCursor
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _mockChatMessages.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<ChatMessage>>(), 
                                                    It.IsAny<FindOptions<ChatMessage, ChatMessage>>(), 
                                                    default))
                             .ReturnsAsync(mockCursor.Object);

            _mockEncryptionHelper.Setup(e => e.Decrypt(It.IsAny<byte[]>()))
                                 .Returns(base64Compressed);

            var result = await _controller.FetchMessages(new FetchMessagesRequest
            {
                UserId = "user1",
                RecipientId = "user2"
            });

            Assert.IsInstanceOf<OkObjectResult>(result);

            var okResult = result as OkObjectResult;
            var returnedMessages = okResult?.Value as List<ChatMessage>;

            Assert.IsNotNull(returnedMessages);
            Assert.That(returnedMessages.Count, Is.EqualTo(2));
            Assert.That(returnedMessages[0].Content, Is.EqualTo("This is the original content"));
            Assert.That(returnedMessages[1].Content, Is.EqualTo("This is the original content"));
        }
    }
}