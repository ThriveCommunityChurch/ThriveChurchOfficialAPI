using System;
using System.Collections.Generic;
using System.Text;

namespace ThriveChurchOfficialAPI.Core
{
    public class SermonSeriesUpdateRequest
    {
        public SermonSeriesUpdateRequest()
        {
            SermonId = null;
            Name = null;
            Thumbnail = null;
            ArtUrl = null;
        }

        /// <summary>
        /// Id of the sermon object in MongoDB
        /// </summary>
        public string SermonId { get; set; }

        /// <summary>
        /// Requested update for the series name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Ends a sermon series for the specified day.
        /// We will ignore the time here.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Updates the direct URL to the thumbnail for this sermon series
        /// </summary>
        public string Thumbnail { get; set; }

        /// <summary>
        /// Updates the StartDate of a sermon series
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Updates the direct URL to the full res art for this sermon series
        /// </summary>
        public string ArtUrl { get; set; }

        public static bool ValidateRequest(SermonSeriesUpdateRequest request)
        {
            if (request == null)
            {
                return false;
            }

            if (request.ArtUrl == null ||
                request.EndDate == null ||
                request.StartDate == null ||
                request.SermonId == null ||
                request.Thumbnail == null)
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
