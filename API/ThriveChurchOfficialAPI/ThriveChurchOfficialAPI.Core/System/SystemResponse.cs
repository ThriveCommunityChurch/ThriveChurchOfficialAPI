using Microsoft.Extensions.Logging;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Generic system response messages that can be used for any reason
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SystemResponse<T>: SystemResponseBase
    {
        /// <summary>
        /// The data from the response, which is of generic type
        /// </summary>
        public T Result { get; set; }

        /// <summary>
        /// Error C'tor
        /// </summary>
        /// <param name="DidError"></param>
        /// <param name="ErrorMsg"></param>
        public SystemResponse(bool DidError, string ErrorMsg)
        {
            HasErrors = DidError;
            ErrorMessage = ErrorMsg;

            SetFileLoggingType();

            if (DidError)
            {
                Logger.LogWarning(string.Format(SystemMessages.BadRequestResponse, ErrorMsg));
                FileLogger.Warn(string.Format(SystemMessages.BadRequestResponse, ErrorMsg));
            }
        }

        /// <summary>
        /// Success C'tor
        /// </summary>
        /// <param name="Value"></param>
        /// <param name="SuccessMsg"></param>
        public SystemResponse(T Value, string SuccessMsg)
        {
            Result = Value;
            SuccessMessage = SuccessMsg;
        }
    }
}