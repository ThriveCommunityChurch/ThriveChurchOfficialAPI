using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Repositories;

namespace ThriveChurchOfficialAPI.Services
{
    public class SermonsService : BaseService, ISermonsService
    {
        private readonly ISermonsRepository _sermonsRepository;

        // the controller cannot have multiple inheritance so we must push it to the service layer
        public SermonsService(IConfiguration Configuration) 
            : base(Configuration)
        {
            // init the repo with the connection string
            _sermonsRepository = new SermonsRepository(Configuration);
        }

        /// <summary>
        /// returns a list of all Passage Objets
        /// </summary>
        public async Task<AllSermonsResponse> GetAllSermons()
        {
            var getAllSermonsResponse = await _sermonsRepository.GetAllSermons();

            // do the business logic here friend

            return getAllSermonsResponse;
        }

        /// <summary>
        /// returns a list of all Passage Objets
        /// </summary>
        public async Task<LiveSermons> GetLiveSermons()
        {
            var getAllSermonsResponse = await _sermonsRepository.GetLiveSermons();

            // do the business logic here friend

            return getAllSermonsResponse;
        }
    }
}
