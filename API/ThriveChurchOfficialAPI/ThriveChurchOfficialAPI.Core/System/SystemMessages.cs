namespace ThriveChurchOfficialAPI.Core
{
    public static class SystemMessages
    {
        #region Configs

        public const string ConfigurationsMustHaveUniqueKeys = "Each configuration value must have a unique key";
        public const string UnableToFindConfigForKey = "Unable to find configuration with key: {0}";
        public const string UnableToFindConfigs = "No configurations found.";
        public const string InvalidConfigForFBPage = "Facebook page configuration improperly formatted. This value should be a number.";
        public const string SocialConfigsCannotContainSpaces = "Social configurations cannot contain spaces.";
        public const string SocialConfigsContainInvalidCharacters = "Social configurations cannot contain the following characters: &, ?, or =";

        #endregion

        #region Validations

        public const string NullProperty = "Property named {0} cannot be null or empty.";
        public const string ConfigNotProperlyFormatted = "{0} configuration contains value that is not properly formatted: {1}.";
        public const string PhoneNumbersCannotContainSpecialCharactersOrSpaces = "Phone number configurations should not contain any special characters or spaces.";
        public const string UnableToFindPropertyForId = "Unable to find {0} with ID: {1}.";
        public const string UnableToUpdatePropertyForId = "Unable to update {0} with ID: {1}.";
        public const string EmptyRequest = "Request cannot be null or empty.";
        public const string InvalidConfigCSVFormat = "Invalid configuration CSV format.";
        public const string ConfigValuesNotFound = "One or more configuration values not found for requested keys.";
        public const string EndDateMustBeAfterStartDate = "EndDate must be an ISO-8601 DateTime after StartDate.";
        public const string InvalidPropertyType = "the property {0} is not the correct type of {1}.";
        public const string ConnectionMissingFromAppSettings = "{0} is a required configuation parameter. Please add it to the AppSettings.json and restart the application.";
        public const string OverrideMissingFromAppSettings = "Attempting to override 'EsvApiKey' within your appsettings.json but were unable to parse the value into a bool. " +
            "Please add \"true\" (to not check for a key) or \"false\" (to check for a key) " +
            "to 'OverrideEsvApiKey' within your AppSettings.json and restart the application.";
        public const string PropertyNameCharactersLengthRange = "Property named {0} must must be within {1} and {2} characters in length.";

        #endregion

        #region Sermon Messages

        public const string UnableToFindValueInCollection = "Unable to find value {0} in the {1} collection.";
        public const string UnableToFindSermonWithSlug = "Unable to find sermon with slug {0}.";
        public const string UnableToFindSermonMessageWithId = "Unable to find sermon message with Id {0}.";
        public const string ErrorOcurredInsertingIntoCollection = "An error ocurred while inserting a document into the {0} collection.";
        public const string ErrorOcurredUpdatingDocumentForKey = "An error ocurred while updating the document with requested key of {0}.";
        public const string UnableToModifySlugForExistingSermonSeries = "Unable to modify slug for sermon series.";
        public const string SeriesWithSlugAlreadyExists = "A series with the slug {0} already exists.";
        public const string UnableToFind = "Unable to find {0}.";
        public const string AudioDurationTooShort = "AudioDuration cannot be less than or equal to 0 seconds.";

        #endregion

        #region Misc

        public const string InvalidPagingNumber = "Unable to return results for page number {0} of a max {1} page(s).";
        public const string IllogicalPagingNumber = "Unable to request results for page number {0}.";
        public const string ExceptionMessage = "UNKNOWN EXCEPTION: {0}, {1}.";
        public const string BadRequestResponse = "Bad Request. {0}";
        public const string UnknownExceptionOcurred = "An unknown error ocurred. Please refer to Id {0}.";

        #endregion

        #region Passages

        public const string ErrorWithESVApi = "An error ocurred while using the ESV API.";

        #endregion
    }
}