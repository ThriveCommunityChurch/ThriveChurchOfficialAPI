using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Services
{
    /// <summary>
    /// Service interface for retrieving sermon transcripts from Azure Blob Storage
    /// </summary>
    public interface ITranscriptService
    {
        /// <summary>
        /// Gets the transcript for a sermon message by its ID
        /// </summary>
        /// <param name="messageId">The unique identifier of the message</param>
        /// <returns>SystemResponse containing the transcript data or error</returns>
        Task<SystemResponse<TranscriptResponse>> GetTranscriptAsync(string messageId);
    }
}

