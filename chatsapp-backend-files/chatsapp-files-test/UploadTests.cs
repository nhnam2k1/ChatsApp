using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MongoDB.Driver;
using FileUploadApi.Controllers;
using Azure.Messaging.ServiceBus;
using System.Text;

namespace FileUploadApi.Tests
{
    [TestFixture]
    public class ChatHubControllerTests
    {
        private Mock<IMongoCollection<ChatMessage>> _mockChatMessagesCollection;
        private Mock<ILogger<ChatHubController>> _mockLogger;
        private Mock<IEncryptionHelper> _mockEncryptionHelper;
        private Mock<IMongoClient> _mockMongoClient;
        private Mock<IMongoDatabase> _mockDatabase;
        private Mock<ServiceBusClient> _mockServiceBusClient;
        private Mock<ServiceBusSender> _mockSender;
        private Mock<ServiceBusSender> _mockChatSender;
        private ChatHubController _controller;

        [SetUp]
        public void SetUp()
        {
            _mockChatMessagesCollection = new Mock<IMongoCollection<ChatMessage>>();
            _mockLogger = new Mock<ILogger<ChatHubController>>();
            _mockEncryptionHelper = new Mock<IEncryptionHelper>();
            _mockMongoClient = new Mock<IMongoClient>();
            _mockDatabase = new Mock<IMongoDatabase>();
            _mockServiceBusClient = new Mock<ServiceBusClient>();
            _mockSender = new Mock<ServiceBusSender>();
            _mockChatSender = new Mock<ServiceBusSender>();

            _mockMongoClient.Setup(client => client.GetDatabase(It.IsAny<string>(), null))
                .Returns(_mockDatabase.Object);
            _mockDatabase.Setup(db => db.GetCollection<ChatMessage>(It.IsAny<string>(), null))
                .Returns(_mockChatMessagesCollection.Object);
            _mockServiceBusClient.Setup(client => client.CreateSender(It.Is<string>(s => s == "filequeue")))
                .Returns(_mockSender.Object);
            _mockServiceBusClient.Setup(client => client.CreateSender(It.Is<string>(s => s == "chat-messages")))
                .Returns(_mockChatSender.Object);

            _controller = new ChatHubController(
                _mockMongoClient.Object,
                _mockServiceBusClient.Object,
                _mockEncryptionHelper.Object,
                _mockLogger.Object
            );
        }

        [Test]
        public async Task UploadFile_FileIsNullOrEmpty_ReturnsBadRequest()
        {
            // Arrange
            var uploadData = JsonSerializer.Serialize(new ChatMessage());

            // Act
            var result = await _controller.UploadFile(It.IsAny<IFormFile>(), uploadData);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult?.Value, Is.EqualTo("No file uploaded."));
        }

        [Test]
        public async Task UploadFile_FileSizeExceedsLimit_ReturnsBadRequest()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(2 * 1024 * 1024); // 2 MB file
            var uploadData = JsonSerializer.Serialize(new ChatMessage());

            // Act
            var result = await _controller.UploadFile(fileMock.Object, uploadData);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult?.Value, Is.EqualTo("File size exceeds 1MB."));
        }

        [Test]
        public async Task UploadFile_InvalidFileType_ReturnsBadRequest()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(500 * 1024); // 500 KB file
            fileMock.Setup(f => f.FileName).Returns("file.exe"); // Invalid file extension
            var uploadData = JsonSerializer.Serialize(new ChatMessage());

            // Act
            var result = await _controller.UploadFile(fileMock.Object, uploadData);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult?.Value, Is.EqualTo("Invalid file type. Only .pdf, .doc, .docx, and .txt files are allowed."));
        }

        [Test]
        public async Task UploadFile_UserIdIsEmpty_ReturnsUnauthorized()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(500 * 1024); // 500 KB file
            fileMock.Setup(f => f.FileName).Returns("file.txt"); // Valid file extension
            var uploadData = JsonSerializer.Serialize(new ChatMessage());

            // Act
            var result = await _controller.UploadFile(fileMock.Object, uploadData);

            // Assert
            Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.That(unauthorizedResult?.Value, Is.EqualTo("User ID not found in request"));
        }

        [Test]
        public async Task UploadFile_RecipientIdIsEmpty_ReturnsUnauthorized()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(500 * 1024); // 500 KB file
            fileMock.Setup(f => f.FileName).Returns("file.txt"); // Valid file extension
            var uploadData = JsonSerializer.Serialize(new ChatMessage { UserId = "user1" });

            // Act
            var result = await _controller.UploadFile(fileMock.Object, uploadData);

            // Assert
            Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.That(unauthorizedResult?.Value, Is.EqualTo("Recipient ID not found in request"));
        }

        [Test]
        public async Task UploadFile_ExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var fileName = "file.txt";
            var fileBytes = Encoding.UTF8.GetBytes("This is a test file");

            fileMock.Setup(f => f.Length).Returns(fileBytes.Length);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(fileBytes));
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns((Stream target, CancellationToken token) =>
                    {
                        return target.WriteAsync(fileBytes, 0, fileBytes.Length, token);
                    });

            var payload = new ChatMessage
            {
                UserId = "user1",
                RecipientId = "user2",
                Content = "Test message"
            };
            var uploadData = JsonSerializer.Serialize(payload);

            _mockEncryptionHelper.Setup(helper => helper.Encrypt(It.IsAny<string>()))
                .Throws(new Exception("Encryption error"));

            // Act
            var result = await _controller.UploadFile(fileMock.Object, uploadData);

            // Assert
            Assert.IsInstanceOf<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.That(objectResult?.StatusCode, Is.EqualTo(500));
            var other = objectResult.Value as ObjectResult;
            Assert.That(other?.Value, Is.EqualTo("Something went wrong, please try again"));
        }
    }
}
