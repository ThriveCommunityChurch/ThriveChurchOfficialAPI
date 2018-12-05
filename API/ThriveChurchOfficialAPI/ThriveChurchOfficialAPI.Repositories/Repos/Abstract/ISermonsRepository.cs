using System.Collections.Generic;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Repositories
{
    public interface ISermonsRepository
    {
        Task<AllSermonsResponse> GetAllSermons();

        Task<LiveSermons> GetLiveSermons();

        Task<LiveSermons> UpdateLiveSermons(LiveSermons request);
    }
}