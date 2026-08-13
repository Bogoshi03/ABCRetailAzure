using Azure.Storage.Queues;
using ABCRetailAzure.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetailAzure.Controllers
{
    public class QueueController : Controller
    {
        private readonly AzureStorageService _azureStorage;

        public QueueController(AzureStorageService azureStorage)
        {
            _azureStorage = azureStorage;
        }

        public async Task<IActionResult> Index()
        {
            var queueClient = _azureStorage.GetQueueClient();

            await queueClient.CreateIfNotExistsAsync();

            var messages = await queueClient.PeekMessagesAsync(
                maxMessages: 32);

            var queueMessages = messages.Value
                .Select(message => message.MessageText)
                .ToList();

            return View(queueMessages);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrder(
            string orderNumber,
            string product,
            int quantity)
        {
            var queueClient = _azureStorage.GetQueueClient();

            await queueClient.CreateIfNotExistsAsync();

            string message =
                $"Order #{orderNumber} - {product} - Quantity: {quantity}";

            await queueClient.SendMessageAsync(message);

            return RedirectToAction(nameof(Index));
        }
    }
}