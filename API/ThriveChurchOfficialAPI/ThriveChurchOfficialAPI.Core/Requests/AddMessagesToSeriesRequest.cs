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
        public IEnumerable<SermonMessageRequest> MessagesToAdd { get; set; }

        /// <summary>
        /// Validate the request object
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static ValidationResponse ValidateRequest(AddMessagesToSeriesRequest request)
        {
            if (request == null)
            {
                return new ValidationResponse(true, SystemMessages.EmptyRequest);
            }

            if (request.MessagesToAdd == null)
            {
                return new ValidationResponse(true, string.Format(SystemMessages.NullProperty, "MessagesToAdd"));
            }

            foreach (var message in request.MessagesToAdd)
            {
                var validateMessages = SermonMessageRequest.ValidateRequest(message);
                if (validateMessages.HasErrors)
                {
                    return new ValidationResponse(true, validateMessages.ErrorMessage);
                }
            }

            return new ValidationResponse("Success!");
        }
    }
}
