using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ThriveChurchOfficialAPI.Repositories;

namespace ThriveChurchOfficialAPI.Services
{
    public class PassagesService : BaseService, IPassagesService
    {
        private readonly IPassagesRepository _passagesRepository;

        public PassagesService(IConfiguration Configuration,
            IPassagesRepository passagesRepository) 
            : base(Configuration)
        {
            _passagesRepository = passagesRepository;
        }

        /// <summary>
        /// returns a list of all Passage Objets
        /// </summary>
        public async Task<string> GetAllPassages()
        {
            Console.Write(EsvApiKey);

            var response = await _passagesRepository.GetAllPassages(EsvApiKey);

            return "Genesis 1";
        }
    }
}
