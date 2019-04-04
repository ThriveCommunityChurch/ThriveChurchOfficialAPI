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
    /// Generic validation response used to validate request objects
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ValidationResponse
    {
        private bool _errored;
        private string _errorMessage;
        private string _successMessage;

        [DataMember]
        public bool HasErrors
        {
            get { return _errored; }
            set { _errored = value; }
        }

        [DataMember]
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }

        [DataMember]
        public string SuccessMessage
        {
            get { return _successMessage; }
            set { _successMessage = value; }
        }

        /// <summary>
        /// Failure C'tor
        /// </summary>
        /// <param name="DidError"></param>
        /// <param name="ErrorMsg"></param>
        public ValidationResponse(bool DidError, string ErrorMsg)
        {
            HasErrors = DidError;
            ErrorMessage = ErrorMsg;
        }

        /// <summary>
        /// Success C'tor
        /// </summary>
        /// <param name="SuccessMsg"></param>
        public ValidationResponse(string SuccessMsg)
        {
            SuccessMessage = SuccessMsg;
        }
    }
}