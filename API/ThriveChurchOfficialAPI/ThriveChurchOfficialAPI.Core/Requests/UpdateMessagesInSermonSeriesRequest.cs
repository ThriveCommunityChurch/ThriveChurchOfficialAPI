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
        public SermonMessage Message { get; set; }

        /// <summary>
        /// Validates the request object
        /// </summary>
        /// <param name="request"></param>
        public static bool ValidateRequest(UpdateMessagesInSermonSeriesRequest request)
        {
            if (request == null)
            {
                return false;
            }

            if (request.Message == null)
            {
                return false;
            }

            var validMessage = SermonMessage.ValidateRequest(request.Message);
            if (!validMessage)
            {
                return false;
            }

            return true;
        }
    }
}
