using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Repositories
{
    /// <summary>
    /// Interface for S3 file operations
    /// </summary>
    public interface IS3Repository
    {
        /// <summary>
        /// Uploads an audio file to S3 bucket
        /// </summary>
        /// <param name="request">The file stream to upload</param>
        /// <returns>SystemResponse containing the S3 URL or error message</returns>
        Task<SystemResponse<string>> UploadAudioFileAsync(HttpRequest request);
    }
}
