using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThriveChurchOfficialAPI
{
    public abstract class BaseService
    {
        // allow any service to be able to retrieve this configuration easily
        public readonly string EsvApiKey;

        public BaseService(IConfiguration Configuration)
        {
            // get the API key from appsettings.json
            EsvApiKey = Configuration["EsvApiKey"];
        }

        public string GetBetween(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "";
            }
        }
    }
}
