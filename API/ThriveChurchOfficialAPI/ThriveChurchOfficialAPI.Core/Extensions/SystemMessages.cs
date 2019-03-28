namespace ThriveChurchOfficialAPI.Core
{
    public static class SystemMessages
    {
        #region Validations

        public static string PropertyRequired
        {
            get { return "The {0} property is required."; }
        }

        public static string NullProperty
        {
            get { return "Property named {0} cannot be null or empty."; }
        }

        public static string EmptyRequest
        {
            get { return "Request cannot be null or empty."; }
        }

        public static string EndDateMustBeAfterStartDate
        {
            get { return "EndDate must be an ISO-8601 DateTime after StartDate."; }
        }

        public static string InvalidPropertyType
        {
            get { return "the property {0} is not the correct type of {1}."; }
        }

        public static string ConnectionMissingFromAppSettings
        {
            get { return "{0} is a required configuation parameter. Please add it to the AppSettings.json and restart the application."; } 
        }

        public static string OverrideMissingFromAppSettings
        {
            get
            {
                return "Attempting to override 'EsvApiKey' within your appsettings.json but were unable to parse the value into a bool. Please add \"true\" (to not check for a key) or \"false\" (to check for a key) " +
                    "to 'OverrideEsvApiKey' within your AppSettings.json and restart the application.";
            }
        }

        #endregion

        #region Sermon Messages

        public static string UnableToFindValueInCollection
        {
            get { return "Unable to find value {0} in the {1} collection."; }
        }

        public static string UnableToFindSermonWithSlug
        {
            get { return "Unable to find sermon with slug {0}."; }
        }

        public static string UnableToFindSermonMessageWithId
        {
            get { return "Unable to find sermon message with Id {0}."; }
        }

        public static string ErrorOcurredInsertingIntoCollection
        {
            get { return "An error ocurred while inserting a document into the {0} collection."; }
        }

        public static string UnableToFindLiveSermonForId
        {
            get { return "Unable to find LiveSermon for Id {0}."; }
        }

        #endregion

        #region Misc

        public static string InvalidPagingNumber
        {
            get { return "Unable to return results for page number {0} of a max {1} page(s)."; }
        }

        public static string IllogicalPagingNumber
        {
            get { return "Unable to request results for page number {0}."; }
        }

        #endregion
    }
}
