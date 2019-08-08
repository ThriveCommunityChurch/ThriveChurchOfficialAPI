using System.Runtime.Serialization;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Base class that stores system responses, including errors and response objects
    /// </summary>
    public class SystemResponseBase
    {
        private bool _errored;
        private string _errorMessage;
        private string _successMessage;

        /// <summary>
        /// Flag for if the returning method encountered some error
        /// </summary>
        [DataMember]
        public bool HasErrors
        {
            get { return _errored; }
            set { _errored = value; }
        }

        /// <summary>
        /// Descriptive error message
        /// </summary>
        [DataMember]
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }

        /// <summary>
        /// Success message
        /// </summary>
        [DataMember]
        public string SuccessMessage
        {
            get { return _successMessage; }
            set { _successMessage = value; }
        }
    }
}
