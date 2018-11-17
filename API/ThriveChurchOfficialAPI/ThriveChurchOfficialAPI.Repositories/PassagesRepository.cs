using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ThriveChurchOfficialAPI.Repositories
{
    public class PassagesRepository: RepositoryBase, IPassagesRepository
    {
        public PassagesRepository()
        {
        }

        public async Task<string> GetPassagesForSearch(string apiKey, string searchCriteria)
        {
            // setup the request
            var uri = string.Format("https://api.esv.org/v3/passage/text/?q={0}", searchCriteria);

            var response = await GetPassages(uri, apiKey);

            // get the passage results from the passage response
            var passageResults = response.passages;

            return passageResults;
        }
    }
}
