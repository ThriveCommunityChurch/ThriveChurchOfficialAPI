namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Indicates which transcript-related features are available for a sermon message.
    /// Use these values to determine which API endpoints can be called for additional content.
    /// </summary>
    public enum TranscriptFeature
    {
        /// <summary>
        /// Full sermon transcript is available via GET /api/sermons/series/message/{id}/transcript
        /// </summary>
        Transcript,

        /// <summary>
        /// AI-generated sermon notes are available via GET /api/sermons/series/message/{id}/notes
        /// </summary>
        Notes,

        /// <summary>
        /// AI-generated study guide is available via GET /api/sermons/series/message/{id}/study-guide
        /// </summary>
        StudyGuide
    }
}

