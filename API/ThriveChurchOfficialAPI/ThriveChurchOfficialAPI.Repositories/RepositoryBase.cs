using System.Net.Http;
using System.Threading.Tasks;

namespace ThriveChurchOfficialAPI.Repositories
{
    public abstract class RepositoryBase
    {
        public RepositoryBase()
        {

        }

        public async Task<HttpResponseMessage> Get(string uri, string authenticationToken)
        {
            var authToken = string.Format("Token {0}", authenticationToken);

            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("User-Agent", "HttpClient");
            request.Headers.Add("Authorization", authToken);

            HttpClient client = new HttpClient();

            var response = await client.SendAsync(request);

            return response;

        }
    }
}