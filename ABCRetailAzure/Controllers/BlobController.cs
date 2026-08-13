using Azure.Storage.Blobs;
using ABCRetailAzure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetailAzure.Controllers
{
    public class BlobController : Controller
    {
        private readonly AzureStorageService _azureStorage;

        public BlobController(AzureStorageService azureStorage)
        {
            _azureStorage = azureStorage;
        }

        public async Task<IActionResult> Index()
        {
            var container = _azureStorage.GetBlobContainerClient();

            await container.CreateIfNotExistsAsync();

            var blobs = new List<string>();

            await foreach (var blob in container.GetBlobsAsync())
            {
                blobs.Add(blob.Name);
            }

            return View(blobs);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                return RedirectToAction(nameof(Index));
            }

            var container = _azureStorage.GetBlobContainerClient();

            await container.CreateIfNotExistsAsync();

            var blobName = Guid.NewGuid().ToString() + "_" + image.FileName;

            var blobClient = container.GetBlobClient(blobName);

            using var stream = image.OpenReadStream();

            await blobClient.UploadAsync(
                stream,
                overwrite: true);

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Display(string name)
        {
            var container = _azureStorage.GetBlobContainerClient();

            var blobClient = container.GetBlobClient(name);

            if (!await blobClient.ExistsAsync())
            {
                return NotFound();
            }

            var response = await blobClient.DownloadAsync();

            return File(
                response.Value.Content,
                response.Value.ContentType);
        }

    }
}
