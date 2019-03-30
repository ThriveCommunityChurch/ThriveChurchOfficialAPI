using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Generic system response messages that can be used for any reason
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SystemResponse<T>
    {
        private bool _errored;
        private string _errorMessage;
        private string _successMessage;

        [DataMember()]
        public bool HasErrors
        {
            get { return _errored; }
            set { _errored = value; }
        }

        [DataMember()]
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }

        [DataMember()]
        public string SuccessMessage
        {
            get { return _successMessage; }
            set { _successMessage = value; }
        }

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