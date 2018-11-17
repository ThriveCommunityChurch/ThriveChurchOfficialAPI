using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThriveChurchOfficialAPI
{
    public class PassageTextInfo
    {
        public PassageTextInfo()
        {

        }

        public string query { get; set; }

        public string canonical { get; set; }

        /// <summary>
        /// this is a strange object because the ESV api returns the passages as one
        /// whole string all together including the footnotes
        /// </summary>
        public string passages { get; set; }
    }
}
