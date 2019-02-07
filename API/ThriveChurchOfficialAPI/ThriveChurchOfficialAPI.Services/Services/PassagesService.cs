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
        public async Task<SermonPassageResponse> GetSinglePassageForSearch(string searchCriteria)
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
            var finalPassage = RemoveFooterTagsAndFormatVerseNumbers(footerRemovalResponse);

            // replace the canonical with what was requested
            finalPassage = finalPassage.Replace(string.Format("{0}\n\n", getPassagesResponse.canonical), "");

            var response = new SermonPassageResponse
            {
                Passage = finalPassage
            };

            return response;
        }

        private string RemoveFooterTagsAndFormatVerseNumbers(string passage)
        {
            var number = "";
            var opened = false;
            var footerNumberList = new List<string>();
            var verseNumberList = new List<string>();
            string SuperscriptDigits = "\u2070\u00b9\u00b2\u00b3\u2074\u2075\u2076\u2077\u2078\u2079";

            foreach (char c in passage)
            {
                if (c == '(')
                {
                    opened = true;
                    continue;
                }
                else if (c == ')')
                {
                    opened = false;

                    var validNumber = int.TryParse(number, out int result);
                    if (validNumber) {
                        footerNumberList.Add(number);
                    }

                    number = "";
                    continue;
                }
                else if (c == '[')
                {
                    opened = true;
                    continue;
                }
                else if (c == ']')
                {
                    opened = false;

                    var validNumber = int.TryParse(number, out int result);
                    if (validNumber)
                    {
                        verseNumberList.Add(number);
                    }

                    number = "";
                    continue;
                }

                if (opened)
                {
                    number += c.ToString();
                }
            }

            foreach (var footnoteTag in footerNumberList)
            {
                passage = passage.Replace(string.Format("({0})", footnoteTag), "");
            }

            foreach (var verseNumberText in verseNumberList)
            {
                string superscript = new string(verseNumberText.Select(i => SuperscriptDigits[i - '0']).ToArray());

                passage = passage.Replace(string.Format("[{0}] ", verseNumberText), superscript);
            }

            return passage;
        }
    }
}
