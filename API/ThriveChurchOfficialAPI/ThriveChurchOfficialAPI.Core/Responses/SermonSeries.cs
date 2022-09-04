using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// C'tor
    /// </summary>
    public class SermonSeries: ObjectBase
    {
        public SermonSeries()
        {
            EndDate = null;
            Name = null;
            Slug = null;
            Thumbnail = null;
            ArtUrl = null;
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// The name of the sermon series
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The starting date of the sermon series - we will ignore the time
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The ending date of the sermon series - we will ignore the time
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// This is a reference to the url link on the website (so these need to stay unique)
        /// for example -> domain.org/{insert-slug-here}
        /// </summary>
        public string Slug { get; set; }

        /// <summary>
        /// The direct URL to the thumbnail for this sermon series
        /// </summary>
        public string Thumbnail { get; set; }

        /// <summary>
        /// The direct URL to the full res art for this sermon series
        /// </summary>
        public string ArtUrl { get; set; }

        /// <summary>
        /// Used as a timestamp to indicate the last time that this object was updated
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }
}