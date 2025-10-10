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
    public class SermonSeriesResponse
    {
        public SermonSeriesResponse()
        {
            StartDate = null;
            EndDate = null;
            Messages = null;
            Name = null;
            Year = null;
            Slug = null;
            Thumbnail = null;
            ArtUrl = null;
            LastUpdated = null;
            Tags = new List<MessageTag>();
            Summary = null;
        }

        /// <summary>
        /// ObjectId notation from Mongo
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The name of the sermon series
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// This is a string notation for the year that the series is taking place
        /// </summary>
        public string Year { get; set; }

        /// <summary>
        /// The starting date of the sermon series - we will ignore the time
        /// </summary>
        public DateTime? StartDate { get; set; }

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
        public DateTime? LastUpdated { get; set; }

        /// <summary>
        /// A collection of Messages spoken / given by someone within this sermon series
        /// </summary>
        public IEnumerable<SermonMessageResponse> Messages { get; set; }

        /// <summary>
        /// A collection of unique tags from all messages in this series, categorizing the series by topic/theme
        /// </summary>
        public IEnumerable<MessageTag> Tags { get; set; }

        /// <summary>
        /// The overall description of the sermon series as a whole
        /// </summary>
        public string Summary { get; set; }
    }
}