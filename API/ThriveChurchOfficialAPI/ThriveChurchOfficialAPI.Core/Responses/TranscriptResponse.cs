namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Response containing transcript data for a sermon message
    /// </summary>
    public class TranscriptResponse
    {
        /// <summary>
        /// C'tor
        /// </summary>
        public TranscriptResponse()
        {
            MessageId = null;
            Title = null;
            Speaker = null;
            FullText = null;
            WordCount = 0;
            Notes = null;
            StudyGuide = null;
        }

        /// <summary>
        /// The unique identifier of the message
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// The title of the sermon message
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The speaker's name
        /// </summary>
        public string Speaker { get; set; }

        /// <summary>
        /// The full transcript text
        /// </summary>
        public string FullText { get; set; }

        /// <summary>
        /// Word count of the transcript
        /// </summary>
        public int WordCount { get; set; }

        /// <summary>
        /// AI-generated sermon notes (optional - may be null if not yet generated)
        /// </summary>
        public SermonNotesResponse Notes { get; set; }

        /// <summary>
        /// AI-generated study guide (optional - may be null if not yet generated)
        /// </summary>
        public StudyGuideResponse StudyGuide { get; set; }
    }
}

