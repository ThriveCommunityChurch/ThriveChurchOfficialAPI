using System;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// C'tor
    /// </summary>
    public class SermonMessage
    {
        public SermonMessage()
        {
            AudioUrl = null;
            VideoUrl = null;
            PassageRef = null;
            Speaker = null;
            Title = null;
            Date = null;
        }

        /// <summary>
        /// The full Url for the .mp3 for the sermon recording.
        /// If null then this may not have been recorded
        /// </summary>
        public string AudioUrl { get; set; }

        /// <summary>
        /// The full Url for the youtube video for the sermon recording.
        /// If null then this may not have been recorded
        /// </summary>
        public string VideoUrl { get; set; }

        /// <summary>
        /// The passage being referenced in this message
        /// </summary>
        public string PassageRef { get; set; }

        /// <summary>
        /// The individual giving this message
        /// </summary>
        public string Speaker { get; set; }

        /// <summary>
        /// The title of the message. If null follow this pattern
        /// {Series Name} - Week {#}
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The date that this message was given - we will ignore the time
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// Validates the object
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool ValidateRequest(SermonMessage request)
        {
            if (request == null)
            {
                return false;
            }

            // A/V urls, and PassageRef are allowed to be null, however others cannot
            if (request.Date == null || 
                request.Speaker == null || 
                request.Title == null)
            {
                return false;
            }

            return true;
        }
    }
}