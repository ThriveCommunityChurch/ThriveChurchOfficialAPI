using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ThriveChurchOfficialAPI.Repositories
{
    public class PassagesRepository: RepositoryBase, IPassagesRepository
    {
        public PassagesRepository(IConfiguration Configuration)
            : base(Configuration)
        {
        }

        public async Task<PassageTextInfo> GetPassagesForSearch(string searchCriteria)
        {
            if (EsvApiKey == null)
            {
                throw new ArgumentNullException("'EsvApiKey' is a required within your appsettings.json in order to continue");
            }

            // setup the request
            var escapedString = Uri.EscapeDataString(searchCriteria);
            var uri = string.Format("https://api.esv.org/v3/passage/text/?q={0}", escapedString);

            var response = await GetPassages(uri, EsvApiKey);

            return response;
        }

        // This should probably return any type that the user requests, however if the API we are using ever breaks the contract 
        public static async Task<PassageTextInfo> GetPassages(string uri, string authenticationToken)
        {
            var authToken = string.Format("Token {0}", authenticationToken);

            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("User-Agent", "HttpClient");
            request.Headers.Add("Authorization", authToken);

            HttpClient client = new HttpClient();

            PassageTextInfo passageAndInfo = null;
            HttpResponseMessage response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                passageAndInfo = JsonConvert.DeserializeObject<PassageTextInfo>(jsonString);
            }

            return passageAndInfo;
        }
    }
}
