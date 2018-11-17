using System.Collections.Generic;
using System.Threading.Tasks;

namespace ThriveChurchOfficialAPI.Repositories
{
    public interface IPassagesRepository
    {
        Task<string> GetPassagesForSearch(string apiKey, string searchCriteria);
    }
}
