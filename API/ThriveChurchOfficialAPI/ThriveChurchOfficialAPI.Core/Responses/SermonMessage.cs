using System;
using System.ComponentModel.DataAnnotations;

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
            AudioDuration = null;
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
        [Url(ErrorMessage = "'AudioUrl' must be in valid url syntax.")]
        [DataType(DataType.Url)]
        public string AudioUrl { get; set; }

        /// <summary>
        /// A numeric value representing the number of seconds of the message audio file
        /// </summary>
        public double? AudioDuration { get; set; }

        /// <summary>
        /// The full Url for the youtube video for the sermon recording.
        /// If null then this may not have been recorded
        /// </summary>
        [Url(ErrorMessage = "'VideoUrl' must be in valid url syntax.")]
        [DataType(DataType.Url)]
        public string VideoUrl { get; set; }

        /// <summary>
        /// The passage being referenced in this message
        /// </summary>
        [Required(ErrorMessage = "No value given for property 'PassageRef'. This property is required.")]
        [DataType(DataType.Text)]
        public string PassageRef { get; set; }

        /// <summary>
        /// The individual giving this message
        /// </summary>
        [Required(ErrorMessage = "No value given for property 'Speaker'. This property is required.")]
        [DataType(DataType.Text)]
        public string Speaker { get; set; }

        /// <summary>
        /// The title of the message. If null follow this pattern
        /// {Series Name} - Week {#}
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "No non-empty value given for property 'Title'. This property is required.")]
        [DataType(DataType.Text)]
        public string Title { get; set; }

        /// <summary>
        /// The date that this message was given - we will ignore the time
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime? Date { get; set; }

        /// <summary>
        /// String representation of a GUID (Cannot be modified)
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// Validates the object
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static ValidationResponse ValidateRequest(SermonMessage request)
        {
            if (request == null)
            {
                return new ValidationResponse(true, SystemMessages.EmptyRequest);
            }

            // A/V urls, and PassageRef are allowed to be null, however others cannot
            if (request.Date == null)
            {
                return new ValidationResponse(true, string.Format(SystemMessages.NullProperty, "Date"));
            }
            
            if (request.Speaker == null)
            {
                return new ValidationResponse(true, string.Format(SystemMessages.NullProperty, "Speaker"));
            }

            if (request.Title == null)
            {
                return new ValidationResponse(true, string.Format(SystemMessages.NullProperty, "Title"));
            }

            if (request?.AudioDuration <= 0)
            {
                return new ValidationResponse(true, SystemMessages.AudioDurationTooShort);
            }

            return new ValidationResponse("Success!");
        }
    }
}