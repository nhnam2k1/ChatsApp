using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace FileUploadApi.Controllers
{
    [Route("chatHub")]
    [ApiController]
    [Authorize]
    public class ChatHubController : ControllerBase
    {
        private readonly IMongoCollection<ChatMessage> _chatMessages;
        private readonly ILogger<ChatHubController> logger;
        private readonly IEncryptionHelper encryptionHelper;
        public ChatHubController(IMongoClient mongoClient, 
                                IEncryptionHelper encryptionHelper,
                                ILogger<ChatHubController> logger)
        {
            this.logger = logger;
            this.encryptionHelper = encryptionHelper;
            var database = mongoClient.GetDatabase("Chatsapp");
            _chatMessages = database.GetCollection<ChatMessage>("ChatMessages");
        }

        [HttpPost("fetch-messages")]
        public async Task<IActionResult> FetchMessages([FromBody] FetchMessagesRequest request)
        {
            try{
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)
                                ?.Value 
                                ?? string.Empty;
            
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID not found in token");
                }

                if (request is null || string.IsNullOrEmpty(request.UserId) 
                    || string.IsNullOrEmpty(request.RecipientId))
                {
                    return BadRequest("Invalid request payload");
                }

                var filter = Builders<ChatMessage>.Filter.Or(
                    Builders<ChatMessage>.Filter.And(
                        Builders<ChatMessage>.Filter.Eq(m => m.UserId, request.UserId),
                        Builders<ChatMessage>.Filter.Eq(m => m.RecipientId, request.RecipientId)
                    ),
                    Builders<ChatMessage>.Filter.And(
                        Builders<ChatMessage>.Filter.Eq(m => m.UserId, request.RecipientId),
                        Builders<ChatMessage>.Filter.Eq(m => m.RecipientId, request.UserId)
                    )
                );

                var sort = Builders<ChatMessage>.Sort.Ascending(m => m.Timestamp);
                List<ChatMessage> messages = await _chatMessages
                                            .Find(filter)
                                            .Sort(sort)
                                            .ToListAsync();
                Parallel.ForEach(messages, 
                    new ParallelOptions { MaxDegreeOfParallelism = 2 },
                    msg => {
                        byte[] encryptedBytes = Convert.FromBase64String(msg.Content);
                        string decryptedContent = encryptionHelper.Decrypt(encryptedBytes);
                        byte[] compressedBytes = Convert.FromBase64String(decryptedContent);
                        msg.Content = CompressionHelper.Decompress(compressedBytes);
                    }
                );

                return Ok(messages);
            }
            catch(Exception e){
                logger.LogError(e.Message);
                ObjectResult objectResult = new("Something went wrong, please try again");
                return StatusCode(500, objectResult);
            }
        }
    }
}