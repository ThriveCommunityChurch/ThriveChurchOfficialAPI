using System;
using System.Threading.Tasks;

namespace ThriveChurchOfficialAPI.Repositories
{
    public class PassagesRepository: RepositoryBase, IPassagesRepository
    {
        public PassagesRepository()
        {
        }

        public async Task<string> GetAllPassages(string apiKey)
        {
            // setup the request
            var uri = "https://api.esv.org/v3/passage/search/?q=rabble";

            var response = await Get(uri, apiKey);

            if (response != null)
            {
                Console.Write(response);
            }

            return "";
        }
    }
}
