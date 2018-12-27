using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Repositories;

namespace ThriveChurchOfficialAPI.Services
{
    public class PassagesService : BaseService, IPassagesService
    {
        private readonly IPassagesRepository _passagesRepository;
        

        // the controller cannot have multiple inheritance so we must push it to the service layer
        public PassagesService(IPassagesRepository passagesRepository)
        {
            _passagesRepository = passagesRepository;
        }

        /// <summary>
        /// returns a list of all Passage Objets
        /// </summary>
        public async Task<PassagesResponse> GetPassagesForSearch(string searchCriteria)
        {
            if (string.IsNullOrEmpty(searchCriteria))
            {
                return default(PassagesResponse);
            }

            searchCriteria = "Psalm 119";

            // since ESV returns everything as one massive string, I need to convert everything to objects
            // Then to strings if I wish
            var getPassagesResponse = await _passagesRepository.GetPassagesForSearch(searchCriteria);

            // this may or may not need to be called several times so this would make sense to move this into the base service
            var convertObjectResponse = ConvertESVTextIntoConsumibleObjects(getPassagesResponse);

            return convertObjectResponse;
        }
    }
}
