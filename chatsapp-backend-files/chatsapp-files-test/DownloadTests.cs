using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MongoDB.Driver;
using FileDownloadApi.Controllers;

namespace ChatApp.Tests
{
    [TestFixture]
    public class ChatHubControllerTests
    {
        private Mock<IMongoCollection<ChatMessage>> _mockChatMessagesCollection;
        private Mock<IMongoCollection<FileMessage>> _mockFileMessagesCollection;
        private Mock<ILogger<ChatHubController>> _mockLogger;
        private Mock<IEncryptionHelper> _mockEncryptionHelper;
        private Mock<IMongoClient> _mockMongoClient;
        private Mock<IMongoDatabase> _mockDatabase;
        private ChatHubController _controller;
        private Mock<IAsyncCursor<FileMessage>> _mockFileCursor;
        private Mock<IAsyncCursor<ChatMessage>> _mockChatCursor;

        [SetUp]
        public void SetUp()
        {
            _mockChatMessagesCollection = new Mock<IMongoCollection<ChatMessage>>();
            _mockFileMessagesCollection = new Mock<IMongoCollection<FileMessage>>();
            _mockLogger = new Mock<ILogger<ChatHubController>>();
            _mockEncryptionHelper = new Mock<IEncryptionHelper>();
            _mockMongoClient = new Mock<IMongoClient>();
            _mockDatabase = new Mock<IMongoDatabase>();
            _mockFileCursor = new Mock<IAsyncCursor<FileMessage>>();
            _mockChatCursor = new Mock<IAsyncCursor<ChatMessage>>();

            _mockMongoClient.Setup(client => client.GetDatabase(It.IsAny<string>(), null))
                .Returns(_mockDatabase.Object);
            _mockDatabase.Setup(db => db.GetCollection<ChatMessage>(It.IsAny<string>(), null))
                .Returns(_mockChatMessagesCollection.Object);
            _mockDatabase.Setup(db => db.GetCollection<FileMessage>(It.IsAny<string>(), null))
                .Returns(_mockFileMessagesCollection.Object);

            _controller = new ChatHubController(
                _mockMongoClient.Object,
                _mockLogger.Object,
                _mockEncryptionHelper.Object
            );
        }

        [Test]
        public async Task DownloadFile_FileIdIsNullOrEmpty_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.DownloadFile("");

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult?.Value, Is.EqualTo("File ID is required."));
        }
        
        [Test]
        public async Task DownloadFile_FileNotFound_ReturnsNotFound()
        {
            // Arrange
            _mockFileCursor.SetupSequence(cursor => cursor.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(false);
            _mockFileMessagesCollection.Setup(collection => collection
                .FindAsync(It.IsAny<FilterDefinition<FileMessage>>(), 
                            It.IsAny<FindOptions<FileMessage>>(), 
                            It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockFileCursor.Object);

            // Act
            var result = await _controller.DownloadFile("non-existent-id");

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult?.Value, Is.EqualTo("File not found."));
        }

        [Test]
        public async Task DownloadFile_ExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var fileId = "valid-file-id";

            _mockFileMessagesCollection.Setup(collection => collection.FindAsync(It.IsAny<FilterDefinition<FileMessage>>(), It.IsAny<FindOptions<FileMessage>>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception("Database error"));

            // Act
            var result = await _controller.DownloadFile(fileId);

            // Assert
            Assert.IsInstanceOf<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.That(objectResult?.StatusCode, Is.EqualTo(500));
            var other = objectResult?.Value as ObjectResult;
            Assert.That(other?.Value, Is.EqualTo("Something went wrong, please try again"));
        }
    }
}