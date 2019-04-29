using log4net;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
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

        private static ILogger _logger { get; set; }

        private static ILog _log { get; set; }

        /// <summary>
        /// Set the type for the file logger based on the class that is calling this
        /// </summary>
        public void SetFileLoggingType()
        {
            StackFrame frame = new StackFrame(2);
            var method = frame.GetMethod();
            var type = method.DeclaringType?.DeclaringType ?? method.DeclaringType ?? typeof(SystemResponseBase);

            _log = LogManager.GetLogger(type);
        }

        /// <summary>
        /// Request response logger
        /// </summary>
        public ILogger Logger
        {
            get { return _logger; }
        }

        public ILog FileLogger
        {
            get { return _log; }
        }

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

        /// <summary>
        /// Setup logger for error responses
        /// </summary>
        /// <param name="logger"></param>
        public static void ConfigureLogger(ILogger logger)
        {
            _logger = logger;
        }
    }
}
