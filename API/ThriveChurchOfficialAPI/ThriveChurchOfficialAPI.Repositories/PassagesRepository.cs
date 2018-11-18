using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThriveChurchOfficialAPI.Repositories
{
    public class PassagesRepository: RepositoryBase, IPassagesRepository
    {
        public PassagesRepository()
        {
        }

        public async Task<PassageTextInfo> GetPassagesForSearch(string apiKey, string searchCriteria)
        {
            // setup the request
            var uri = string.Format("https://api.esv.org/v3/passage/text/?q={0}", searchCriteria);

            var response = await GetPassages(uri, apiKey);

            return response;
        }
    }
}
