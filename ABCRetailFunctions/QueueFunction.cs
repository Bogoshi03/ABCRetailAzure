using System;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ABCRetailFunctions
{
    public class QueueFunction
    {
        private readonly ILogger<QueueFunction> _logger;

        public QueueFunction(ILogger<QueueFunction> logger)
        {
            _logger = logger;
        }

        [Function("WriteToQueueStorage")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("Queue Storage function was triggered.");

            string action = req.Query["action"];

            string connectionString =
                Environment.GetEnvironmentVariable("AzureStorage");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return new StatusCodeResult(500);
            }

            var queueClient = new QueueClient(
                connectionString,
                "function-orders");

            await queueClient.CreateIfNotExistsAsync();

            if (action?.ToLower() == "read")
            {
                var messages = await queueClient.PeekMessagesAsync(1);

                if (messages.Value.Length == 0)
                {
                    return new OkObjectResult("The queue is currently empty.");
                }

                return new OkObjectResult(
                    $"Message read from Azure Queue Storage: {messages.Value[0].MessageText}");
            }

            string message = req.Query["message"];

            if (string.IsNullOrWhiteSpace(message))
            {
                message = "ABC Retail order transaction received.";
            }

            await queueClient.SendMessageAsync(message);

            return new OkObjectResult(
                $"Message successfully written to Azure Queue Storage: {message}");
        }
    }
}
