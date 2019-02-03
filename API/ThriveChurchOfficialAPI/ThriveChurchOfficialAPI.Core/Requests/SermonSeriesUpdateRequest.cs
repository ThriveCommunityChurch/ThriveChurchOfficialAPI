using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ThriveChurchOfficialAPI.Core
{
    public class SermonSeriesUpdateRequest
    {
        public SermonSeriesUpdateRequest()
        {
            Name = null;
            Thumbnail = null;
            ArtUrl = null;
            Slug = null;
        }

        /// <summary>
        /// Requested update for the series name
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "No non-empty value given for property 'Name'. This property is required.")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        /// <summary>
        /// Ends a sermon series for the specified day.
        /// We will ignore the time here.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "No non-empty value given for property 'EndDate'. This property is required.")]
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Updates the direct URL to the thumbnail for this sermon series
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "No non-empty value given for property 'Thumbnail'. This property is required.")]
        [Url(ErrorMessage = "'Thumbnail' must be in valid url syntax.")]
        [DataType(DataType.ImageUrl)]
        public string Thumbnail { get; set; }

        /// <summary>
        /// Updates the StartDate of a sermon series
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "No non-empty value given for property 'StartDate'. This property is required.")]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Updates the direct URL to the full res art for this sermon series
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "No non-empty value given for property 'ArtUrl'. This property is required.")]
        [Url(ErrorMessage = "'ArtUrl' must be in valid url syntax.")]
        [DataType(DataType.ImageUrl)]
        public string ArtUrl { get; set; }

        /// <summary>
        /// Updates the slug text for this sermon series
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "No non-empty value given for property 'Slug'. This property is required.")]
        [DataType(DataType.Text)]
        public string Slug { get; set; }

        public static bool ValidateRequest(SermonSeriesUpdateRequest request)
        {
            if (request == null)
            {
                return false;
            }

            if (request.ArtUrl == null ||
                request.EndDate == null ||
                request.StartDate == null ||
                request.Thumbnail == null ||
                request.Slug == null)
            {
                return false;
            }

            // make sure that the dates are chronological
            if (request.StartDate > request.EndDate)
            {
                return false;
            }

            return true;
        }
    }
}
