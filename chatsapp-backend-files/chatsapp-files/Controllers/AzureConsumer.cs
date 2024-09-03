using System.Text.Json;
using Azure.Messaging.ServiceBus;
using MongoDB.Driver;

namespace AzureServiceBusConsumerWebApi.Services
{
    public class AzureServiceBusConsumerService : BackgroundService
    {
        private readonly ILogger<AzureServiceBusConsumerService> _logger;
        private readonly IMongoCollection<FileMessage> _fileMessages;
        private readonly string _queueName = "filequeue";
        private readonly ServiceBusClient client;
        private readonly IEncryptionHelper encryptionHelper;
        private ServiceBusProcessor _processor;
        public AzureServiceBusConsumerService(ILogger<AzureServiceBusConsumerService> logger,
                                            IEncryptionHelper encryptionHelper,
                                            ServiceBusClient client, IMongoClient mongoClient)
        {
            _logger = logger;
            this.client = client;
            this.encryptionHelper = encryptionHelper;
            _processor = client.CreateProcessor(_queueName, new ServiceBusProcessorOptions());

            var database = mongoClient.GetDatabase("Chatsapp");
            _fileMessages = database.GetCollection<FileMessage>("FileMessages");
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _processor = client.CreateProcessor(_queueName, new ServiceBusProcessorOptions());

            _processor.ProcessMessageAsync += ProcessMessagesAsync;
            _processor.ProcessErrorAsync += ProcessErrorAsync;

            await _processor.StartProcessingAsync(cancellationToken);

            _logger.LogInformation("Azure Service Bus Consumer Service started.");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // No need to implement this method as the processor is started in StartAsync
            return Task.CompletedTask;
        }

        private async Task ProcessMessagesAsync(ProcessMessageEventArgs args)
        {
            try {
                var message = args.Message;
                var body = message.Body.ToString();
                if (string.IsNullOrEmpty(body)) return;
                
                FileMessage fileMessage = JsonSerializer.Deserialize<FileMessage>(body)
                                        ?? new FileMessage();

                if (string.IsNullOrEmpty(fileMessage.Id) 
                || fileMessage.File.Length == 0) return;

                var fileStr = Convert.ToBase64String(fileMessage.File);
                var encryptedBytes = encryptionHelper.Encrypt(fileStr);
                fileMessage.File = encryptedBytes;
                await _fileMessages.InsertOneAsync(fileMessage);
            }
            catch(Exception e)
            {
                _logger.LogError(e.Message);
            }
            finally
            {
                // Complete the message. Remove it from the queue.
                await args.CompleteMessageAsync(args.Message);
            }
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs args)
        {
            _logger.LogError($"Message handler encountered an exception {args.Exception}.");
            _logger.LogDebug($"Error Source: {args.ErrorSource}");
            _logger.LogDebug($"Entity Path: {args.EntityPath}");
            _logger.LogDebug($"FullyQualifiedNamespace: {args.FullyQualifiedNamespace}");

            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Azure Service Bus Consumer Service is stopping.");
            await _processor.StopProcessingAsync(cancellationToken);
            await _processor.DisposeAsync();
        }
    }
}