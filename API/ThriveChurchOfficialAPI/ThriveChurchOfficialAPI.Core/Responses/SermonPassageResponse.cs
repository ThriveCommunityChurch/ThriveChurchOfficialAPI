using System;
using System.Collections.Generic;
using System.Text;

namespace ThriveChurchOfficialAPI.Core
{
    public class SermonPassageResponse
    {
        public SermonPassageResponse()
        {
            Passage = null;
        }

        /// <summary>
        /// The pre-formatted passage that the user requested
        /// </summary>
        public string Passage { get; set; }
    }
}
