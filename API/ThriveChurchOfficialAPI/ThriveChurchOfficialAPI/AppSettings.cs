namespace ThriveChurchOfficialAPI
{
    public class AppSettings
    {
        /// <summary>
        /// Gets the API Key from ESV Api within the Appsettings.json
        /// </summary>
        public string EsvApiKey { get; set; }

        /// <summary>
        /// Use this within your AppSettings.json to set the path for your mongoDB
        /// </summary>
        public string MongoConnectionString { get; set; }
    }
}