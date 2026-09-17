using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ABCRetailFunctions
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }

        [Function("StoreCustomerInTable")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("Table Storage function was triggered.");

            string name = req.Query["name"];
            string email = req.Query["email"];

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
            {
                return new BadRequestObjectResult(
                    "Please provide name and email.");
            }

            string connectionString =
                Environment.GetEnvironmentVariable("AzureStorage");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return new StatusCodeResult(500);
            }

            var serviceClient = new TableServiceClient(connectionString);

            var tableClient = serviceClient.GetTableClient("Customers");

            await tableClient.CreateIfNotExistsAsync();

            var customer = new TableEntity(
                "FunctionCustomers",
                Guid.NewGuid().ToString())
            {
                ["Name"] = name,
                ["Email"] = email,
                ["CreatedAt"] = DateTime.UtcNow
            };

            await tableClient.AddEntityAsync(customer);

            return new OkObjectResult(
                $"Customer '{name}' was successfully stored in Azure Table Storage.");
        }
    }
}