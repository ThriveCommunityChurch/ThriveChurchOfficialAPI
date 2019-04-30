using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ThriveChurchOfficialAPI.Repositories
{
    /// <summary>
    /// Passages Repo
    /// </summary>
    public class PassagesRepository: RepositoryBase, IPassagesRepository
    {
        /// <summary>
        /// Passages Repo C'tor
        /// </summary>
        /// <param name="Configuration"></param>
        public PassagesRepository(IConfiguration Configuration, ITokenRepo tokenRepo)
            : base(Configuration, tokenRepo)
        {
        }

        /// <summary>
        /// Gets passages from ESV API given some search criteria
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public async Task<PassageTextInfo> GetPassagesForSearch(string searchCriteria)
        {
            if (string.IsNullOrEmpty(EsvApiKey))
            {
                throw new ArgumentNullException("'EsvApiKey' is a required within your appsettings.json in order to continue.");
            }

            // setup the request
            var escapedString = Uri.EscapeDataString(searchCriteria);
            var uri = string.Format("https://api.esv.org/v3/passage/text/?q={0}", escapedString);

            var response = await GetPassages(uri, EsvApiKey);

            return response;
        }

        /// <summary>
        /// Send the request to the ESV API with our auth token
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="authenticationToken"></param>
        /// <returns></returns>
        public async Task<PassageTextInfo> GetPassages(string uri, string authenticationToken)
        {
            var authToken = string.Format("Token {0}", authenticationToken);

            var response = await GetAsync(uri, authToken);
            PassageTextInfo passageAndInfo = null;

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                passageAndInfo = JsonConvert.DeserializeObject<PassageTextInfo>(jsonString);
            }

            return passageAndInfo;
        }
    }
}
