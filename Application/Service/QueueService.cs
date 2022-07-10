using Application.IService;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Service
{
    public class QueueService : IQueueService
    {
        private readonly string serviceConnectionString;
        private readonly string queueName;

        private readonly IConfiguration _configuration;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ServiceBusSender sender;

        public QueueService(IConfiguration configuration)
        {
            _configuration = configuration;
            serviceConnectionString = _configuration["AzureServiceBus:ConnectionStrings"];
            queueName = _configuration["AzureServiceBus:QueueName"];

            _serviceBusClient = new ServiceBusClient(serviceConnectionString);
            sender = _serviceBusClient.CreateSender(queueName);
        }

        public async Task SenderAsync(string message)
        {
            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

            if (!messageBatch.TryAddMessage(new ServiceBusMessage($"{message}")))
            {
                // if it is too large for the batch
                throw new Exception($"The message {message} is too large to fit in the batch.");
            }

            try
            {
                // Use the producer client to send the batch of messages to the Service Bus queue
                await sender.SendMessagesAsync(messageBatch);
                Console.WriteLine($"A batch of messages has been published to the queue.");
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await sender.DisposeAsync();
                await _serviceBusClient.DisposeAsync();
            }
        }
    }

    public class MessageHandler : BackgroundService
    {
        private readonly string serviceConnectionString;
        private readonly string queueName;

        private readonly IConfiguration _configuration;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ServiceBusProcessor processor;

        public MessageHandler(IConfiguration configuration)
        {
            _configuration = configuration;
            serviceConnectionString = _configuration["AzureServiceBus:ConnectionStrings"];
            queueName = _configuration["AzureServiceBus:QueueName"];

            _serviceBusClient = new ServiceBusClient(serviceConnectionString);
            processor = _serviceBusClient.CreateProcessor(queueName, new ServiceBusProcessorOptions());
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                // add handler to process messages
                processor.ProcessMessageAsync += HandleMessage;
                // add handler to process any errors
                processor.ProcessErrorAsync += ErrorHandler;
                while (true)
                {
                    // start processing 
                    await processor.StartProcessingAsync();

                    // stop processing 
                    await processor.StopProcessingAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await processor.DisposeAsync();
                await _serviceBusClient.DisposeAsync();
            }
        }

        private async Task HandleMessage(ProcessMessageEventArgs args)
        {
            string message = args.Message.Body.ToString();

            // complete the message. message is deleted from the queue. 
            await args.CompleteMessageAsync(args.Message);
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
    }
}
