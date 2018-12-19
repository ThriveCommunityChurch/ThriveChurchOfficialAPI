using System;
using System.Collections.Generic;
using System.Text;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// C'tor
    /// </summary>
    public class AllSermonsResponse
    {
        // TODO: make this an abbreviated response that only contains things like:
        // SeriesId, MessageIds[], Title, Date, IsActive, etc so that this isn't a huge response 
        public AllSermonsResponse()
        {
            Sermons = null;
        }

        /// <summary>
        /// A collection of all the sermons
        /// </summary>
        public IEnumerable<SermonSeries> Sermons { get; set; }
    }
}
