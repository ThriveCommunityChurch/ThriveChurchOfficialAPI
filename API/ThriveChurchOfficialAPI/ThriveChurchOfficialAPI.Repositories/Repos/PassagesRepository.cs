using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThriveChurchOfficialAPI.Repositories
{
    public class PassagesRepository: RepositoryBase, IPassagesRepository
    {
        private readonly string EsvApiKey;

        public PassagesRepository(IConfiguration Configuration)
        {
            // get the API key from appsettings.json
            EsvApiKey = Configuration["EsvApiKey"];
        }

        public async Task<PassageTextInfo> GetPassagesForSearch(string searchCriteria)
        {
            if (EsvApiKey == null)
            {
                throw new Exception("'EsvApiKey' is a required within your appsettings.json in order to continue");
            }

            // setup the request
            var escapedString = Uri.EscapeUriString(searchCriteria);

            // Apparently Colons are ignored by the RFC or something
            escapedString = escapedString.Replace(":", "%3A");
            var uri = string.Format("https://api.esv.org/v3/passage/text/?q={0}", escapedString);

            var response = await GetPassages(uri, EsvApiKey);

            return response;
        }
    }
}
