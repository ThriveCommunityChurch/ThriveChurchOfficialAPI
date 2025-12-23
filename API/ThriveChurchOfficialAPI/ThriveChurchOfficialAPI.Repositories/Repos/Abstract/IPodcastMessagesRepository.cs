using System.Collections.Generic;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Repositories
{
    /// <summary>
    /// Repository interface for PodcastMessages collection
    /// </summary>
    public interface IPodcastMessagesRepository
    {
        /// <summary>
        /// Gets all podcast messages from the PodcastMessages collection
        /// </summary>
        /// <returns>Collection of all podcast messages</returns>
        Task<List<PodcastMessage>> GetAllPodcastMessages();

        /// <summary>
        /// Gets a podcast message by its unique Id
        /// </summary>
        /// <param name="messageId">The MongoDB ObjectId of the message</param>
        /// <returns>The podcast message if found, error response otherwise</returns>
        Task<SystemResponse<PodcastMessage>> GetPodcastMessageById(string messageId);

        /// <summary>
        /// Updates an existing podcast message by its Id
        /// </summary>
        /// <param name="messageId">The MongoDB ObjectId of the message to update</param>
        /// <param name="message">The updated podcast message data</param>
        /// <returns>The updated podcast message if successful, null otherwise</returns>
        Task<PodcastMessage> UpdatePodcastMessageById(string messageId, PodcastMessageRequest message);
    }
}

