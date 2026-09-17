using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Files.Shares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ABCRetailFunctions
{
    public class FileFunction
    {
        private readonly ILogger<FileFunction> _logger;

        public FileFunction(ILogger<FileFunction> logger)
        {
            _logger = logger;
        }

        [Function("SendFileToAzureFiles")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("Azure Files function was triggered.");

            string fileName = req.Query["fileName"];
            string content = req.Query["content"];

            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = "ABC-Retail-Function-Log.txt";
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                content = "ABC Retail transaction log created by Azure Function.";
            }

            string connectionString =
                Environment.GetEnvironmentVariable("AzureStorage");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return new StatusCodeResult(500);
            }

            var shareClient = new ShareClient(
                connectionString,
                "function-files");

            await shareClient.CreateIfNotExistsAsync();

            var directoryClient = shareClient.GetRootDirectoryClient();

            var fileClient = directoryClient.GetFileClient(fileName);

            byte[] data = Encoding.UTF8.GetBytes(content);

            using var stream = new MemoryStream(data);

            await fileClient.CreateAsync(stream.Length);

            await fileClient.UploadAsync(stream);

            return new OkObjectResult(
                $"File '{fileName}' was successfully sent to Azure Files.");
        }
    }
}
