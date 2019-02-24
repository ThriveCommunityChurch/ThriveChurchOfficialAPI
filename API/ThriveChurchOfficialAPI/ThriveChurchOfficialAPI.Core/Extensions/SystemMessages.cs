namespace ThriveChurchOfficialAPI.Core
{
    public static class SystemMessages
    {
        #region Validations

        public static string PropertyRequired
        {
            get { return "No value given for property {0}. This property is required."; }
        }

        public static string ConnectionMissingFromAppSettings
        {
            get { return "{0} is a required configuation parameter. Please add it to the AppSettings.json and restart the application."; } 
        }

        #endregion
    }
}
