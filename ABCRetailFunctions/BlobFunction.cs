using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ABCRetailFunctions
{
    public class BlobFunction
    {
        private readonly ILogger<BlobFunction> _logger;

        public BlobFunction(ILogger<BlobFunction> logger)
        {
            _logger = logger;
        }

        [Function("WriteToBlobStorage")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("Blob Storage function was triggered.");

            string fileName = req.Query["fileName"];
            string content = req.Query["content"];

            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = $"product-{Guid.NewGuid()}.txt";
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                content = "ABC Retail product information stored using Azure Blob Storage.";
            }

            string connectionString =
                Environment.GetEnvironmentVariable("AzureStorage");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return new StatusCodeResult(500);
            }

            var blobServiceClient = new BlobServiceClient(connectionString);

            var containerClient =
                blobServiceClient.GetBlobContainerClient("function-blobs");

            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(fileName);

            byte[] data = Encoding.UTF8.GetBytes(content);

            using var stream = new System.IO.MemoryStream(data);

            await blobClient.UploadAsync(
                stream,
                new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = "text/plain"
                    }
                });

            return new OkObjectResult(
                $"File '{fileName}' was successfully written to Azure Blob Storage.");
        }
    }
}
