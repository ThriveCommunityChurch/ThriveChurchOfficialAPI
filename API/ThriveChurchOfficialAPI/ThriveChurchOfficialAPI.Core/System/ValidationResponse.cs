using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Serilog;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Generic validation response used to validate request objects
    /// </summary>
    public class ValidationResponse: SystemResponseBase
    {
        /// <summary>
        /// Failure C'tor
        /// </summary>
        /// <param name="DidError"></param>
        /// <param name="ErrorMsg"></param>
        public ValidationResponse(bool DidError, string ErrorMsg)
        {
            HasErrors = DidError;
            ErrorMessage = ErrorMsg;

            if (DidError)
            {
                Log.Warning(string.Format(SystemMessages.BadRequestResponse, ErrorMsg));
            }
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