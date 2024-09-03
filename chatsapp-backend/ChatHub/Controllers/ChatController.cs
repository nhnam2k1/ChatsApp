using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;

namespace ChatHub.Controllers
{
    [Authorize]
    public class ChatService : Hub
    {
        private readonly IMongoCollection<ChatMessage> _chatMessages;
        private readonly ILogger<ChatService> logger;
        private readonly IEncryptionHelper encryptionHelper;
        public ChatService(IMongoClient mongoClient, 
                            IEncryptionHelper encryptionHelper,
                            ILogger<ChatService> logger)
        {
            this.logger = logger;
            this.encryptionHelper = encryptionHelper;
            var database = mongoClient.GetDatabase("Chatsapp");
            _chatMessages = database.GetCollection<ChatMessage>("ChatMessages");
        }

        public async Task SendMessage(string user, string message)
        {
            try{
                // This is the 'sub' claim from Auth0
                string senderId = Context.UserIdentifier 
                                ?? String.Empty; 
                string receiverId = user;

                if (string.IsNullOrEmpty(senderId) 
                || string.IsNullOrEmpty(receiverId)) return;

                // Compression
                byte[] compressedMessage = CompressionHelper.Compress(message);
                byte[] encryptedMessage = encryptionHelper
                            .Encrypt(Convert.ToBase64String(compressedMessage));
                string contentToSend = Convert.ToBase64String(encryptedMessage);

                var chatMessage = new ChatMessage {
                    UserId = senderId,
                    RecipientId = receiverId,
                    Content = contentToSend,
                    Timestamp = DateTime.UtcNow
                };
                
                await _chatMessages.InsertOneAsync(chatMessage);
                var chatMessageDTO = chatMessage;
                chatMessageDTO.Content = message;
                await Clients.User(receiverId)
                    .SendAsync("ReceiveMessage", senderId, chatMessageDTO);
            }
            catch(Exception e){
                logger.LogError(e.Message);
            }
        }
    }
}