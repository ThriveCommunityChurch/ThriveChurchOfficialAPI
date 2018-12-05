using System.Collections.Generic;

namespace ThriveChurchOfficialAPI
{
    public class PassageTextInfo
    {
        public PassageTextInfo()
        {
            passage_meta = null;
            parsed = null;
            passages = null;
        }

        public string query { get; set; }

        public string canonical { get; set; }

        /// <summary>
        /// this is a strange object because the ESV api returns the passages as one
        /// whole string all together including the footnotes
        /// </summary>
        public IEnumerable<string> passages { get; set; }

        /// <summary>
        /// this seems silly to me to have a 2D datatype here.
        /// Not sure how that's useful
        /// </summary>
        public IEnumerable<int[]> parsed { get; set; }

        public IEnumerable<PassageMetadata> passage_meta { get; set; }
    }
}
