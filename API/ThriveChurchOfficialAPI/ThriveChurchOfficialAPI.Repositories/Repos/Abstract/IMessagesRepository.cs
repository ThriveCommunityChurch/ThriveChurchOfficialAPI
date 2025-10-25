using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Repositories
{
    public interface IMessagesRepository
    {
        /// <summary>
        /// Adds a new Message to the Messages Collection
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        Task<SystemResponse<IEnumerable<SermonMessage>>> CreateNewMessages(IEnumerable<SermonMessage> messages);

        /// <summary>
        /// Gets a message using the specified Id
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        Task<SystemResponse<SermonMessage>> GetMessageById(string messageId);

        /// <summary>
        /// Gets a message using the specified (dual inclusive) date range
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        Task<IEnumerable<SermonMessage>> GetMessageByDateRange(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Gets a collection of messages using their series Id references
        /// </summary>
        /// <param name="seriesId"></param>
        /// <returns></returns>
        Task<IEnumerable<SermonMessage>> GetMessagesBySeriesId(string seriesId);

        /// <summary>
        /// Gets a sermon message using its Id
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        Task<SermonMessage> UpdateMessagePlayCount(string messageId);

        /// <summary>
        /// Get all messages
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<SermonMessage>> GetAllMessages();

        /// <summary>
        /// Updates a message using its unique Id
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<SermonMessage> UpdateMessageById(string messageId, SermonMessageRequest message);

        /// <summary>
        /// Search for messages that contain at least one of the specified tags
        /// </summary>
        /// <param name="tags">Tags to search for</param>
        /// <param name="sortDirection">Sort direction by date</param>
        /// <returns>Collection of matching messages as SermonMessageResponse</returns>
        Task<IEnumerable<SermonMessageResponse>> SearchMessagesByTags(IEnumerable<MessageTag> tags, SortDirection sortDirection);

        /// <summary>
        /// Gets all unique speaker names from sermon messages
        /// </summary>
        /// <returns>Collection of unique speaker names</returns>
        Task<IEnumerable<string>> GetUniqueSpeakers();

        /// <summary>
        /// Search for messages by speaker name
        /// </summary>
        /// <param name="speaker">Speaker name to search for (case-insensitive)</param>
        /// <param name="sortDirection">Sort direction by date</param>
        /// <returns>Collection of matching messages</returns>
        Task<IEnumerable<SermonMessage>> SearchMessagesBySpeaker(string speaker, SortDirection sortDirection);
    }
}