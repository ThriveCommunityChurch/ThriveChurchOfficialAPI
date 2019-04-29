namespace ThriveChurchOfficialAPI.Core
{
    public static class SystemMessages
    {
        #region Validations

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

        public static string NoApiKey
        {
            get
            {
                return "Authorization failed." ;
            }
        }

        public static string WrongApiKey
        {
            get
            {
                return "Requested API Key is invalid.";
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

        public static string ErrorOcurredUpdatingDocumentForKey
        {
            get { return "An error ocurred while updating the document with requested key of {0}."; }
        }

        public static string UnableToModifySlugForExistingSermonSeries
        {
            get { return "Unable to modify slug for sermon series."; }
        }

        public static string SeriesWithSlugAlreadyExists
        {
            get { return "A series with the slug {0} already exists."; }
        }

        public static string UnableToFindLiveSermon
        {
            get { return "Unable to find LiveSermon."; }
        }

        public static string AudioDurationTooShort
        {
            get { return "AudioDuration cannot be less than or equal to 0 seconds."; }
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

        public static string ExceptionMessage
        {
            get { return "UNKNOWN EXCEPTION: {0}, {1}."; }
        }

        public static string BadRequestResponse
        {
            get { return "Bad Request. {0}"; }
        }

        public static string InvalidAPIKeyDebug
        {
            get { return "Unauthorized. {0} attempted to make a requiest using the api key: {1}"; }
        }

        public static string NoAPIKeyDebug
        {
            get { return "Unauthorized. {0} attempted to make a requiest without an API Key."; }
        }

        public static string UnknownExceptionOcurred
        {
            get { return "An unknown error ocurred. Please refer to Id {0}."; }
        }

        #endregion

        #region Passages

        public static string ErrorWithESVApi
        {
            get { return "An error ocurred while using the ESV API."; }
        }

        #endregion
    }
}
