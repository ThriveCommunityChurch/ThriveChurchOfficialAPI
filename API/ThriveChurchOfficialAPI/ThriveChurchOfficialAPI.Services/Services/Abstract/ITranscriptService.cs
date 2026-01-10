using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Services
{
    /// <summary>
    /// Service interface for retrieving sermon transcripts, notes, and study guides from Azure Blob Storage
    /// </summary>
    public interface ITranscriptService
    {
        /// <summary>
        /// Gets the transcript for a sermon message by its ID (including notes and study guide if available)
        /// </summary>
        /// <param name="messageId">The unique identifier of the message</param>
        /// <returns>SystemResponse containing the transcript data or error</returns>
        Task<SystemResponse<TranscriptResponse>> GetTranscriptAsync(string messageId);

        /// <summary>
        /// Gets just the sermon notes for a message by its ID
        /// </summary>
        /// <param name="messageId">The unique identifier of the message</param>
        /// <returns>SystemResponse containing the sermon notes or error (404 if notes not yet generated)</returns>
        Task<SystemResponse<SermonNotesResponse>> GetSermonNotesAsync(string messageId);

        /// <summary>
        /// Gets just the study guide for a message by its ID
        /// </summary>
        /// <param name="messageId">The unique identifier of the message</param>
        /// <returns>SystemResponse containing the study guide or error (404 if guide not yet generated)</returns>
        Task<SystemResponse<StudyGuideResponse>> GetStudyGuideAsync(string messageId);
    }
}

