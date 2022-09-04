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
    }
}