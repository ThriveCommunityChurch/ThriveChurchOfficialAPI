using System.Threading.Tasks;

namespace ThriveChurchOfficialAPI.Services
{
    /// <summary>
    /// Abstraction for blob storage operations to enable testing
    /// </summary>
    public interface IBlobStorageClient
    {
        /// <summary>
        /// Checks if a blob exists in the container
        /// </summary>
        /// <param name="blobName">Name of the blob to check</param>
        /// <returns>True if the blob exists, false otherwise</returns>
        Task<bool> BlobExistsAsync(string blobName);

        /// <summary>
        /// Downloads the content of a blob as a string
        /// </summary>
        /// <param name="blobName">Name of the blob to download</param>
        /// <returns>The blob content as a string</returns>
        Task<string> DownloadBlobContentAsync(string blobName);
    }
}

