using System;
using System.Collections.Generic;
using System.Text;

namespace ThriveChurchOfficialAPI.Core
{
    public static class AppMessages
    {
        // Making these all static read-only
        
        public static string PropertyRequired
        {
          get { return "No value given for property {0}. This property is required."; }
        }
    }
}
