using System.Text.Json;
using Azure.Messaging.ServiceBus;
using ChatHub.Controllers;
using Microsoft.AspNetCore.SignalR;

public class ServiceBusListener : BackgroundService
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ServiceBusListener> _logger;
    private readonly string _queueName = "chat-messages";
    private ServiceBusProcessor _processor;

    public ServiceBusListener(ServiceBusClient serviceBusClient, 
                            IServiceProvider serviceProvider, 
                            ILogger<ServiceBusListener> logger)
    {
        _serviceBusClient = serviceBusClient;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _processor = _serviceBusClient
                .CreateProcessor(_queueName, new ServiceBusProcessorOptions());
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _processor = _serviceBusClient
                .CreateProcessor(_queueName, new ServiceBusProcessorOptions());
        
        _processor.ProcessMessageAsync += ProcessMessagesAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;

        await _processor.StartProcessingAsync(cancellationToken);
        _logger.LogInformation("Service Bus Listener started.");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // No implementation needed here, the StartAsync method sets up the processing.
        return Task.CompletedTask;
    }

    private async Task ProcessMessagesAsync(ProcessMessageEventArgs args)
    {
        try {
            var jsonMessage = args.Message.Body.ToString();
            ChatMessage chatMessage = JsonSerializer.Deserialize<ChatMessage>(jsonMessage) 
                                    ?? new ChatMessage();
            
            string receiverId = chatMessage.RecipientId;
            string userId = chatMessage.UserId;

            if (string.IsNullOrEmpty(receiverId) 
            || string.IsNullOrEmpty(userId)) return;

            using var scope = _serviceProvider.CreateScope();
            var chatHub = scope.ServiceProvider
                        .GetRequiredService<IHubContext<ChatService>>();
            await chatHub.Clients
                        .User(receiverId)
                        .SendAsync("ReceiveMessage", userId, chatMessage);
        }
        catch(Exception e)
        {
            _logger.LogError(e.Message);
        }
        finally
        {
            await args.CompleteMessageAsync(args.Message);
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError($"Message handler encountered an exception {args.Exception}.");
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _processor.StopProcessingAsync(cancellationToken);
        await _processor.DisposeAsync();
        _logger.LogInformation("Service Bus Listener stopped.");
    }
}