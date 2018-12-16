using System;
using System.Collections.Generic;
using System.Text;

namespace ThriveChurchOfficialAPI.Core
{
    public class AddMessagesToSeriesRequest
    {
        public AddMessagesToSeriesRequest()
        {
             MessagesToAdd = null;
        }

        /// <summary>
        /// A collection of messages that should be added to this Sermon Series
        /// </summary>
        public IEnumerable<SermonMessage> MessagesToAdd { get; set; }

        /// <summary>
        /// Validate the request object
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool ValidateRequest(AddMessagesToSeriesRequest request)
        {
            if (request == null)
            {
                return false;
            }

            if (request.MessagesToAdd == null)
            {
                return false;
            }

            foreach (var message in request.MessagesToAdd)
            {
                var validateMessages = SermonMessage.ValidateRequest(message);
                if (!validateMessages)
                {
                    // at least one message is invalid
                    return false;
                }
            }

            return true;
        }
    }
}
