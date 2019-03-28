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
        private bool _errored = false;
        private string _message;

        [DataMember()]
        public bool HasErrors
        {
            get { return _errored; }
            set { _errored = value; }
        }

        [DataMember()]
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        /// <summary>
        /// The data from the response, which is of generic type
        /// </summary>
        public T Result { get; set; }

        public SystemResponse(bool DidError, string ErrorMessage)
        {
            HasErrors = DidError;
            Message = ErrorMessage;
        }

        public SystemResponse(T Value, string SuccessMessage)
        {
            Result = Value;
            Message = SuccessMessage;
        }
    }
}