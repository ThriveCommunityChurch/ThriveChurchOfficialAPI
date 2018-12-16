using System;
using System.Collections.Generic;
using System.Text;

namespace ThriveChurchOfficialAPI.Core
{
    public class UpdateMessagesInSermonSeriesRequest
    {
        public UpdateMessagesInSermonSeriesRequest()
        {

        }

        /// <summary>
        /// Id of the Sermon Series
        /// </summary>
        public string SeriesId { get; set; }
    }
}
