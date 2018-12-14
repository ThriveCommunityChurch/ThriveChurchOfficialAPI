using System;
using System.Collections.Generic;
using System.Text;

namespace ThriveChurchOfficialAPI.Core
{
    public class AddMessagesToSeriesRequest
    {
        public AddMessagesToSeriesRequest()
        {
            SeriesId = null;
            MessagesToAdd = null;
        }

        /// <summary>
        /// Id of the series for which to add this message to
        /// </summary>
        public string SeriesId { get; set; }

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

            if (request.MessagesToAdd == null || string.IsNullOrEmpty(request.SeriesId))
            {
                return false;
            }

            return true;
        }
    }
}
