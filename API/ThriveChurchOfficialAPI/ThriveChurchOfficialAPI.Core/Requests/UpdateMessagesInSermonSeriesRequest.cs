using System;
using System.Collections.Generic;
using System.Text;

namespace ThriveChurchOfficialAPI.Core
{
    public class UpdateMessagesInSermonSeriesRequest
    {
        public UpdateMessagesInSermonSeriesRequest()
        {
            Message = null;
        }

        /// <summary>
        /// Requested message update
        /// </summary>
        public SermonMessageRequest Message { get; set; }

        /// <summary>
        /// Validates the request object
        /// </summary>
        /// <param name="request"></param>
        public static ValidationResponse ValidateRequest(UpdateMessagesInSermonSeriesRequest request)
        {
            if (request == null)
            {
                return new ValidationResponse(true, SystemMessages.EmptyRequest);
            }

            if (request.Message == null)
            {
                return new ValidationResponse(true, string.Format(SystemMessages.NullProperty, "Message"));
            }

            var messageValidationResponse = SermonMessageRequest.ValidateRequest(request.Message);
            if (messageValidationResponse.HasErrors)
            {
                return new ValidationResponse(true, messageValidationResponse.ErrorMessage);
            }

            return new ValidationResponse("Success!");
        }
    }
}
