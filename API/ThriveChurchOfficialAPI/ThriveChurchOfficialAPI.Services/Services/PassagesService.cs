using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
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
        public async Task<string> GetSinglePassageForSearch(string searchCriteria)
        {
            if (string.IsNullOrEmpty(searchCriteria))
            {
                return null;
            }

            // since ESV returns everything as one massive string, I need to convert everything to objects
            // Then to strings if I wish
            var getPassagesResponse = await _passagesRepository.GetPassagesForSearch(searchCriteria);

            if (getPassagesResponse == null)
            {
                return null;
            }

            var passageResponse = getPassagesResponse.passages.FirstOrDefault();
            var footerRemovalResponse = RemoveFooterFromResponse(passageResponse);
            var finalPassage = RemoveFooterTags(footerRemovalResponse);

            return finalPassage;
        }

        private string RemoveFooterTags(string passage)
        {

        }
    }
}
