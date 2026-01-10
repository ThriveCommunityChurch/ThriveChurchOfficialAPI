using System.Collections.Generic;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// API response containing sermon notes data
    /// </summary>
    public class SermonNotesResponse
    {
        /// <summary>
        /// The title of the sermon message
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The speaker's name
        /// </summary>
        public string Speaker { get; set; }

        /// <summary>
        /// The date of the sermon
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// The main scripture passage for the sermon
        /// </summary>
        public string MainScripture { get; set; }

        /// <summary>
        /// A 2-3 sentence summary capturing the sermon's core message
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Key points from the sermon with scripture references
        /// </summary>
        public List<KeyPointResponse> KeyPoints { get; set; }

        /// <summary>
        /// Memorable quotes from the sermon
        /// </summary>
        public List<QuoteResponse> Quotes { get; set; }

        /// <summary>
        /// Practical application points from the sermon
        /// </summary>
        public List<string> ApplicationPoints { get; set; }

        /// <summary>
        /// ISO timestamp when the notes were generated
        /// </summary>
        public string GeneratedAt { get; set; }

        /// <summary>
        /// The AI model used to generate the notes
        /// </summary>
        public string ModelUsed { get; set; }

        /// <summary>
        /// Word count of the original transcript
        /// </summary>
        public int WordCount { get; set; }
    }
}

