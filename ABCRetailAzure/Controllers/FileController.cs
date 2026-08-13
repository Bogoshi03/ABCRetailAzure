using ABCRetailAzure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetailAzure.Controllers
{
    public class FileController : Controller
    {
        private readonly AzureStorageService _azureStorage;

        public FileController(AzureStorageService azureStorage)
        {
            _azureStorage = azureStorage;
        }

        public async Task<IActionResult> Index()
        {
            var shareClient = _azureStorage.GetFileShareClient();

            await shareClient.CreateIfNotExistsAsync();

            var directoryClient = shareClient
                .GetRootDirectoryClient();

            var files = new List<string>();

            await foreach (var item in directoryClient.GetFilesAndDirectoriesAsync())
            {
                if (!item.IsDirectory)
                {
                    files.Add(item.Name);
                }
            }

            return View(files);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile logFile)
        {
            if (logFile == null || logFile.Length == 0)
            {
                return RedirectToAction(nameof(Index));
            }

            var shareClient = _azureStorage.GetFileShareClient();

            await shareClient.CreateIfNotExistsAsync();

            var directoryClient = shareClient
                .GetRootDirectoryClient();

            var fileName =
                Guid.NewGuid().ToString() + "_" + logFile.FileName;

            var fileClient =
                directoryClient.GetFileClient(fileName);

            await fileClient.CreateAsync(logFile.Length);

            using var stream = logFile.OpenReadStream();

            await fileClient.UploadAsync(
                stream,
                new Azure.Storage.Files.Shares.Models.ShareFileUploadOptions());

            return RedirectToAction(nameof(Index));
        }
    }
}
