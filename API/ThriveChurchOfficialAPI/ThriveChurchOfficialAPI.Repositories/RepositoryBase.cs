using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
namespace ThriveChurchOfficialAPI.Repositories
{
    public abstract class RepositoryBase
    { 
        protected RepositoryBase()
        {
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