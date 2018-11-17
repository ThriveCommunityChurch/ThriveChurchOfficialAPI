using System;
using System.Collections.Generic;
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
        public PassagesService(IConfiguration Configuration,
            IPassagesRepository passagesRepository) 
            : base(Configuration)
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

            var getPassagesResponse = await _passagesRepository.GetPassagesForSearch(EsvApiKey, searchCriteria);

            var passages = GetBetween(getPassagesResponse, searchCriteria, "Footnotes");
            var footnotes = GetBetween(getPassagesResponse, "Footnotes", "(ESV)");

            // we now need to split them both and return a list of each 
            var passageList = passages.Split('[');
            var footnoteList = footnotes.Split('(');

            // we will need to store these objects in a list so we can return them easily
            var passagesList = new List<Passage>();
            var footnotesList = new List<Footnote>();

            foreach (var passage in passageList)
            {
                // we will need to get the verse # from the beginning of this string
                // however the verse number might be 1-3 digits so we need to split by ]
                var passageSplit = passage.Split(']');
                var passageNumber = Int32.TryParse(passageSplit[0], out int x);

                var passageToAdd = new Passage()
                {
                    Verse = passage,
                    VerseNumber = x
                };
                passagesList.Add(passageToAdd);
            }

            foreach (var footnote in footnoteList)
            {
                // we will need to get the verse # from the beginning of this string
                // however the verse number might be 1-3 digits so we need to split by ]
                var footnoteSplit = footnote.Split(')');
                var passageNumber = Int32.TryParse(footnoteSplit[0], out int x);

                var footnoteToAdd = new Footnote()
                {
                    FootnoteInfo = footnote,
                    VerseNumber = x
                };
                footnotesList.Add(footnoteToAdd);
            }

            var response = new PassagesResponse()
            {
                Canonical = searchCriteria,
                Footnotes = footnotesList,
                Passages = passagesList
            };


            return response;
        }
    }
}
