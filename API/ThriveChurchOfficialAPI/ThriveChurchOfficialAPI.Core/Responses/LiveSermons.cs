using MongoDB.Bson;
using System;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// C'tor
    /// </summary>
    public class LiveSermons
    {
        public LiveSermons()
        {
            IsLive = false;
            VideoUrlSlug = null;
            Title = null;
        }

        /// <summary>
        /// ObjectId notation from Mongo
        /// </summary>
        public ObjectId _id { get; set; }

        /// <summary>
        /// Flag determining whether or not a stream is currently active
        /// </summary>
        public bool IsLive { get; set; }

        /// <summary>
        /// Slug to be used for facebook
        /// EX) facebook.com/{pageName}/videos/{insert-slug-here}
        /// </summary>
        public string VideoUrlSlug { get; set; }

        /// <summary>
        /// The title of the video / message
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Set a time when a livestream lotification will appear,
        /// after this time when a user loads the page the notification will disappear.
        /// We will only use the time here.
        /// </summary>
        public DateTime ExpirationTime { get; set; }
    }
}
