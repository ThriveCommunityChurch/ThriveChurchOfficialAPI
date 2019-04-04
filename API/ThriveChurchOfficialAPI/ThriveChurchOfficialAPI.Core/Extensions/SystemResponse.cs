using System.Runtime.Serialization;

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

        public SystemResponse(bool DidError, string ErrorMsg)
        {
            HasErrors = DidError;
            ErrorMessage = ErrorMsg;
        }

        public SystemResponse(T Value, string SuccessMsg)
        {
            Result = Value;
            SuccessMessage = SuccessMsg;
        }
    }
}