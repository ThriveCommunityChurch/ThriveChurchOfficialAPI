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
            // setup the request
            var uri = string.Format("https://api.esv.org/v3/passage/text/?q={0}", searchCriteria);

            var response = await GetPassages(uri, EsvApiKey);

            return response;
        }
    }
}
