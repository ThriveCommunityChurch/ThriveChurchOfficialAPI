namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// API response for a quote from the sermon
    /// </summary>
    public class QuoteResponse
    {
        /// <summary>
        /// The actual quote text from the sermon
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Optional context about when/why this quote was said
        /// </summary>
        public string Context { get; set; }
    }
}

