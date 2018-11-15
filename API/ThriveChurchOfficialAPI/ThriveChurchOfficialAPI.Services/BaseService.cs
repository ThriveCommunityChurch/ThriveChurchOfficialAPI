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
    }
}
