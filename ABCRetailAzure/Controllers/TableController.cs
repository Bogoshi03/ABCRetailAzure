using Azure.Data.Tables;
using ABCRetailAzure.Models;
using ABCRetailAzure.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetailAzure.Controllers
{
    public class TableController : Controller
    {
        private readonly AzureStorageService _azureStorage;

        public TableController(AzureStorageService azureStorage)
        {
            _azureStorage = azureStorage;
        }

        public async Task<IActionResult> Index()
        {
            var tableClient = _azureStorage.GetTableClient();

            await tableClient.CreateIfNotExistsAsync();

            var records = new List<TableEntity>();

            await foreach (var entity in tableClient.QueryAsync<TableEntity>())
            {
                records.Add(entity);
            }

            return View(records);
        }

        [HttpPost]
        public async Task<IActionResult> Add(
            string customerName,
            string email,
            string product,
            decimal price)
        {
            var tableClient = _azureStorage.GetTableClient();

            await tableClient.CreateIfNotExistsAsync();

            var entity = new TableEntity(
                "Retail",
                Guid.NewGuid().ToString())
            {
                ["CustomerName"] = customerName,
                ["Email"] = email,
                ["Product"] = product,
                ["Price"] = price
            };

            await tableClient.AddEntityAsync(entity);

            return RedirectToAction(nameof(Index));
        }
    }
}
