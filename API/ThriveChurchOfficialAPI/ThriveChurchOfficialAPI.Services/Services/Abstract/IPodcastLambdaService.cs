using System.Threading.Tasks;

namespace ThriveChurchOfficialAPI.Services
{
    /// <summary>
    /// Service for invoking the Podcast RSS Generator Lambda function
    /// </summary>
    public interface IPodcastLambdaService
    {
        /// <summary>
        /// Triggers a full rebuild of the podcast RSS feed
        /// </summary>
        /// <returns>True if the Lambda was successfully invoked</returns>
        Task<bool> RebuildFeedAsync();

        /// <summary>
        /// Upserts a single episode in the podcast RSS feed
        /// </summary>
        /// <param name="messageId">The MongoDB ObjectId of the message to upsert</param>
        /// <param name="skipTranscription">If true, skips audio transcription and reuses existing transcript (use when only metadata changed, not audio)</param>
        /// <returns>True if the Lambda was successfully invoked</returns>
        Task<bool> UpsertEpisodeAsync(string messageId, bool skipTranscription = false);
    }
}
