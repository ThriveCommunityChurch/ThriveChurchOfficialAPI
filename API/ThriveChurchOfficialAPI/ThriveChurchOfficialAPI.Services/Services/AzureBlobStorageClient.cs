using Azure.Storage.Blobs;
using Serilog;
using System.Threading.Tasks;

namespace ThriveChurchOfficialAPI.Services
{
    /// <summary>
    /// Azure Blob Storage implementation of IBlobStorageClient
    /// </summary>
    public class AzureBlobStorageClient : IBlobStorageClient
    {
        private readonly BlobContainerClient _containerClient;

        /// <summary>
        /// Constructor for AzureBlobStorageClient
        /// </summary>
        /// <param name="connectionString">Azure Storage connection string</param>
        /// <param name="containerName">Name of the blob container</param>
        public AzureBlobStorageClient(string connectionString, string containerName)
        {
            _containerClient = new BlobContainerClient(connectionString, containerName);
            Log.Information("AzureBlobStorageClient initialized with container: {ContainerName}", containerName);
        }

        /// <inheritdoc/>
        public async Task<bool> BlobExistsAsync(string blobName)
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            var response = await blobClient.ExistsAsync();
            return response.Value;
        }

        /// <inheritdoc/>
        public async Task<string> DownloadBlobContentAsync(string blobName)
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            var downloadResponse = await blobClient.DownloadContentAsync();
            return downloadResponse.Value.Content.ToString();
        }
    }
}

