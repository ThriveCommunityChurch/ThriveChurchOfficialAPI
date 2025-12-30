namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Represents a physical location for an event
    /// </summary>
    public class EventLocation
    {
        /// <summary>
        /// C'tor
        /// </summary>
        public EventLocation()
        {
            Name = null;
            Address = null;
            City = null;
            State = null;
            ZipCode = null;
            Latitude = null;
            Longitude = null;
        }

        /// <summary>
        /// The name of the location (e.g., 'Main Sanctuary', 'Fellowship Hall')
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The street address of the location
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// The city where the location is situated
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// The state/province where the location is situated
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// The postal/zip code of the location
        /// </summary>
        public string ZipCode { get; set; }

        /// <summary>
        /// The latitude coordinate for map display (optional)
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// The longitude coordinate for map display (optional)
        /// </summary>
        public double? Longitude { get; set; }
    }
}

