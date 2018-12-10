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
