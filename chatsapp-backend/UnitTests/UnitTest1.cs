using Moq;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Microsoft.AspNetCore.SignalR;
using System.Text;
using ChatHub.Controllers;

namespace ChatServiceTests
{
    public class ChatServiceTests
    {
        private Mock<IMongoClient> _mockMongoClient;
        private Mock<IMongoDatabase> _mockDatabase;
        private Mock<IMongoCollection<ChatMessage>> _mockCollection;
        private Mock<IEncryptionHelper> _mockEncryptionHelper;
        private Mock<ILogger<ChatService>> _mockLogger;
        private Mock<IHubCallerClients> _mockClients;
        private Mock<IClientProxy> _mockClientProxy;
        private ChatService _chatService;

        [SetUp]
        public void Setup()
        {
            _mockMongoClient = new Mock<IMongoClient>();
            _mockDatabase = new Mock<IMongoDatabase>();
            _mockCollection = new Mock<IMongoCollection<ChatMessage>>();
            _mockEncryptionHelper = new Mock<IEncryptionHelper>();
            _mockLogger = new Mock<ILogger<ChatService>>();
            _mockClients = new Mock<IHubCallerClients>();
            _mockClientProxy = new Mock<IClientProxy>();

            _mockMongoClient.Setup(client => client.GetDatabase(It.IsAny<string>(), null))
                            .Returns(_mockDatabase.Object);
            _mockDatabase.Setup(db => db.GetCollection<ChatMessage>(It.IsAny<string>(), null))
                         .Returns(_mockCollection.Object);

            _mockClients.Setup(clients => clients.User(It.IsAny<string>()))
                        .Returns(_mockClientProxy.Object);

            _chatService = new ChatService(
                _mockMongoClient.Object,
                _mockEncryptionHelper.Object,
                _mockLogger.Object)
            {
                Clients = _mockClients.Object
            };
        }

        [Test]
        public async Task SendMessage_ShouldSendCorrectMessage()
        {
            // Arrange
            var user = "receiverUser";
            var message = "Hello, World!";
            var senderId = "senderUser";
            var receiverId = "receiverUser";

            var mockContext = new Mock<HubCallerContext>();
            mockContext.Setup(c => c.UserIdentifier).Returns(senderId);
            _chatService.Context = mockContext.Object;

            var compressedMessage = Encoding.UTF8.GetBytes("compressedMessage");
            var encryptedMessage = Convert.FromBase64String("encryptedMessage");

            _mockEncryptionHelper.Setup(eh => eh.Encrypt(It.IsAny<string>()))
                                 .Returns(encryptedMessage);

            // Act
            await _chatService.SendMessage(user, message);

            // Assert
            _mockCollection.Verify(c => c.InsertOneAsync(
                It.IsAny<ChatMessage>(), 
                It.IsAny<InsertOneOptions>(), 
                It.IsAny<CancellationToken>()), 
                Times.Once);

            _mockClientProxy.Verify(client => client.SendCoreAsync(
                "ReceiveMessage", 
                It.Is<object[]>(o => o.Length == 2 && 
                                    (string)o[0] == senderId && 
                                    o[1] is ChatMessage), 
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Test]
        public async Task SendMessage_ShouldNotSendMessage_WhenSenderIdIsEmpty()
        {
            // Arrange
            var user = "receiverUser";
            var message = "Hello, World!";

            var mockContext = new Mock<HubCallerContext>();
            mockContext.Setup(c => c.UserIdentifier).Returns(string.Empty);
            _chatService.Context = mockContext.Object;

            // Act
            await _chatService.SendMessage(user, message);

            // Assert
            _mockCollection.Verify(c => c.InsertOneAsync(
                It.IsAny<ChatMessage>(), 
                It.IsAny<InsertOneOptions>(), 
                It.IsAny<CancellationToken>()), 
                Times.Never);

            _mockClientProxy.Verify(client => client.SendCoreAsync(
                It.IsAny<string>(), 
                It.IsAny<object[]>(), 
                It.IsAny<CancellationToken>()), 
                Times.Never);
        }

        [Test]
        public async Task SendMessage_ShouldNotSendMessage_WhenReceiverIdIsEmpty()
        {
            // Arrange
            var user = string.Empty;
            var message = "Hello, World!";
            var senderId = "senderUser";

            var mockContext = new Mock<HubCallerContext>();
            mockContext.Setup(c => c.UserIdentifier).Returns(senderId);
            _chatService.Context = mockContext.Object;

            // Act
            await _chatService.SendMessage(user, message);

            // Assert
            _mockCollection.Verify(c => c.InsertOneAsync(
                It.IsAny<ChatMessage>(), 
                It.IsAny<InsertOneOptions>(), 
                It.IsAny<CancellationToken>()), 
                Times.Never);

            _mockClientProxy.Verify(client => client.SendCoreAsync(
                It.IsAny<string>(), 
                It.IsAny<object[]>(), 
                It.IsAny<CancellationToken>()), 
                Times.Never);
        }
    }
}