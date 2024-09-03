using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Text.Json;

namespace FileUploadApi.Controllers
{
    [Route("chatHub")]
    [ApiController]
    [Authorize]
    public class ChatHubController : ControllerBase
    {
        private readonly IMongoCollection<ChatMessage> _chatMessages;
        private static readonly string[] allowedExtensions = new[] { 
            ".pdf", ".doc", ".docx", ".txt" 
        };
        private static readonly long MAX_FILE_SIZE = 1 * 1024 * 1024;
        private readonly string _queueName = "filequeue";
        private readonly string _queueChat = "chat-messages";
        private readonly ServiceBusSender sender;
        private readonly ServiceBusSender chatSender;
        private readonly ILogger<ChatHubController> logger;
        private readonly IEncryptionHelper encryptionHelper;
        public ChatHubController(IMongoClient mongoClient, 
                                ServiceBusClient client,
                                IEncryptionHelper encryptionHelper,
                                ILogger<ChatHubController> logger)
        {
            sender = client.CreateSender(_queueName);
            chatSender = client.CreateSender(_queueChat);
            this.logger = logger;
            this.encryptionHelper = encryptionHelper;
            var database = mongoClient.GetDatabase("Chatsapp");
            _chatMessages = database.GetCollection<ChatMessage>("ChatMessages");
        }

        [HttpPost("files")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file, 
                                                    [FromForm] string uploadData)
        {
            try {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded.");
                }

                if (file.Length > MAX_FILE_SIZE)
                {
                    return BadRequest("File size exceeds 1MB.");
                }

                var fileName = file.FileName;
                var fileExtension = Path.GetExtension(fileName)
                                        .ToLowerInvariant();

                if (string.IsNullOrEmpty(fileExtension) 
                || !allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Invalid file type. Only .pdf, .doc, .docx, and .txt files are allowed.");
                }

                var payload = JsonSerializer.Deserialize<ChatMessage>(uploadData) 
                            ?? new ChatMessage();
      
                if (string.IsNullOrEmpty(payload.UserId))
                {
                    return Unauthorized("User ID not found in request");
                }

                if (string.IsNullOrEmpty(payload.RecipientId))
                {
                    return Unauthorized("Recipient ID not found in request");
                }

                // Compression
                byte[] compressedMessage = CompressionHelper.Compress(fileName);
                byte[] encryptedMessage = encryptionHelper
                            .Encrypt(Convert.ToBase64String(compressedMessage));
                string contentToSend = Convert.ToBase64String(encryptedMessage);

                payload.IsAttachment = true;
                payload.Timestamp = DateTime.Now;
                payload.Content = contentToSend;
                var resultTask = _chatMessages.InsertOneAsync(payload);

                // Read file data
                byte[] fileBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }

                await resultTask;
                var fileMessage = new FileMessage{
                    Id = payload.Id,
                    File = fileBytes
                };
                
                //Console.WriteLine($"Upload, Length: {fileMessage.File.Length} and ID: {fileMessage.Id}");
                var jsonMessage = JsonSerializer.Serialize(fileMessage);
                ServiceBusMessage busMessage = new ServiceBusMessage(jsonMessage);
                var fileBusTask = sender.SendMessageAsync(busMessage);
                
                var receivedPayload = payload;
                receivedPayload.Content = fileName;

                var jsonChatMessage = JsonSerializer.Serialize(receivedPayload);
                ServiceBusMessage busChatMessage = new ServiceBusMessage(jsonChatMessage);
                var chatBusTask = chatSender.SendMessageAsync(busChatMessage);

                await Task.WhenAll(fileBusTask, chatBusTask);
                return Ok(receivedPayload);
            }
            catch(Exception e){
                logger.LogError(e.Message);
                ObjectResult objectResult = new("Something went wrong, please try again");
                return StatusCode(500, objectResult);
            }
        }
    }
}