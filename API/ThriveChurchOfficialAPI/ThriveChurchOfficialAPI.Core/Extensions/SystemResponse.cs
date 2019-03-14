using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace ThriveChurchOfficialAPI.Core.Extensions
{
    public class SystemResponse<T>
    {
        private T _data;
        private bool _success = true;
        private string _message;

        [DataMember()]
        public bool WasSuccessful
        {
            get { return _success; }
            set { _success = value; }
        }

        [DataMember()]
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        [DataMember()]
        public T Response
        {
            get { return _data; }
            set { _data = value; }
        }

        public void RecordResponseObject(ref T data)
        {
            _data = data;
        }
    }
}