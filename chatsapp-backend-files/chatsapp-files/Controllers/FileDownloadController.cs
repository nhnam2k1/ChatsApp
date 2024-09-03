using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace FileDownloadApi.Controllers
{
    [Route("chatHub")]
    [ApiController]
    [Authorize]
    public class ChatHubController : ControllerBase
    {
        private readonly IMongoCollection<ChatMessage> _chatMessages;
        private readonly IMongoCollection<FileMessage> _fileMessages;
        private readonly ILogger<ChatHubController> logger;
        private readonly IEncryptionHelper encryptionHelper;
        public ChatHubController(IMongoClient mongoClient, 
                                ILogger<ChatHubController> logger,
                                IEncryptionHelper encryptionHelper)
        {
            var database = mongoClient.GetDatabase("Chatsapp");
            _chatMessages = database.GetCollection<ChatMessage>("ChatMessages");
            _fileMessages = database.GetCollection<FileMessage>("FileMessages");
            this.logger = logger;
            this.encryptionHelper = encryptionHelper;
        }

        [HttpGet("files/{id}")]
        public async Task<IActionResult> DownloadFile(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest("File ID is required.");
                }

                // Retrieve the FileMessage from MongoDB
                var filter = Builders<FileMessage>
                            .Filter.Eq(fm => fm.Id, id);
                var fileMessage = await _fileMessages.Find(filter)
                                        .FirstOrDefaultAsync();

                if (fileMessage is null)
                {
                    return NotFound("File not found.");
                }

                var messageFilter = Builders<ChatMessage>
                                .Filter.Eq(chat => chat.Id, id);
                var chatMessage = await _chatMessages.Find(messageFilter)
                                        .FirstOrDefaultAsync() 
                                        ?? new ChatMessage();
                string userID = User
                        .FindFirst(ClaimTypes.NameIdentifier)
                        ?.Value 
                        ?? String.Empty;
                if (string.IsNullOrEmpty(userID))
                {
                    return NotFound("File not found.");
                }

                if (!chatMessage.UserId.Equals(userID) 
                && !chatMessage.RecipientId.Equals(userID))
                {
                    return NotFound("File not found.");
                }

                byte[] encryptedBytes = Convert.FromBase64String(chatMessage.Content);
                string decryptedContent = encryptionHelper.Decrypt(encryptedBytes);
                byte[] compressedBytes = Convert.FromBase64String(decryptedContent);
                byte[] decompressedBytes = CompressionHelper.Decompress(compressedBytes);
                chatMessage.Content = Encoding.UTF8.GetString(decompressedBytes);

                string decryptedFile = encryptionHelper.Decrypt(fileMessage.File);
                var binary = Convert.FromBase64String(decryptedFile);

                // Return the file as a download
                return File(binary, "application/octet-stream", 
                            $"{chatMessage.Content}");
            }
            catch(Exception e){
                logger.LogError(e.Message);
                ObjectResult objectResult = new("Something went wrong, please try again");
                return StatusCode(500, objectResult);
            }
        }
    }
}