using System;
using System.Collections.Generic;
using MongoDB.Bson;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// C'tor
    /// </summary>
    public class SermonSeries
    {
        public SermonSeries()
        {
            StartDate = null;
            EndDate = null;
            Messages = null;
            Name = null;
            Year = null;
            Slug = null;
            Thumbnail = null;
            ArtUrl = null;
        }

        /// <summary>
        /// ObjectId notation from Mongo
        /// </summary>
        public ObjectId _id { get; set; }

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
        /// This is a reference to the url link on the website 
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
        /// A collection of Messages spoken / given by someone within this sermon series
        /// </summary>
        public IEnumerable<SermonMessage> Messages { get; set; }
    }
}