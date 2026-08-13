using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Storage.Files.Shares;

namespace ABCRetailAzure.Services
{
    public class AzureStorageService
    {
        private readonly string _connectionString;

        public AzureStorageService(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("AzureStorage")
                ?? throw new InvalidOperationException(
                    "Azure Storage connection string is missing.");
        }

        // Azure Table Storage
        public TableClient GetTableClient()
        {
            var serviceClient = new TableServiceClient(_connectionString);

            return serviceClient.GetTableClient("RetailData");
        }

        // Azure Blob Storage
        public BlobContainerClient GetBlobContainerClient()
        {
            var blobServiceClient =
                new BlobServiceClient(_connectionString);

            return blobServiceClient.GetBlobContainerClient(
                "product-images");
        }

        // Azure Queue Storage
        public QueueClient GetQueueClient()
        {
            var queueServiceClient =
                new QueueServiceClient(_connectionString);

            return queueServiceClient.GetQueueClient(
                "order-processing");
        }

        // Azure File Storage
        public ShareClient GetFileShareClient()
        {
            var shareServiceClient =
                new ShareServiceClient(_connectionString);

            return shareServiceClient.GetShareClient(
                "system-logs");
        }
    }
}
