using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public async Task<SystemResponse<SermonPassageResponse>> GetSinglePassageForSearch(string searchCriteria)
        {
            if (string.IsNullOrEmpty(searchCriteria))
            {
                return new SystemResponse<SermonPassageResponse>(true, string.Format(SystemMessages.NullProperty, "searchCriteria"));
            }

            // since ESV returns everything as one massive string, I need to convert everything to objects
            // Then to strings if I wish
            var getPassagesResponse = await _passagesRepository.GetPassagesForSearch(searchCriteria);
            if (getPassagesResponse == null)
            {
                return new SystemResponse<SermonPassageResponse>(true, SystemMessages.ErrorWithESVApi);
            }

            var passageResponse = getPassagesResponse.passages.FirstOrDefault();
            if (passageResponse == null)
            {
                return new SystemResponse<SermonPassageResponse>(true, SystemMessages.ErrorWithESVApi);
            }

            var footerRemovalResponse = RemoveFooterFromResponse(passageResponse);
            var finalPassage = RemoveFooterTagsAndFormatVerseNumbers(footerRemovalResponse);

            // replace the canonical with what was requested
            finalPassage = finalPassage.Replace(string.Format("{0}\n\n", getPassagesResponse.canonical), "");

            var response = new SermonPassageResponse
            {
                Passage = finalPassage
            };

            return new SystemResponse<SermonPassageResponse>(response, "Success!");
        }

        private string RemoveFooterTagsAndFormatVerseNumbers(string passage)
        {
            var builder = new StringBuilder();
            var opened = false;
            var footerNumberList = new List<string>();
            var verseNumberList = new List<string>();
            string SuperscriptDigits = "\u2070\u00b9\u00b2\u00b3\u2074\u2075\u2076\u2077\u2078\u2079";

            foreach (char c in passage)
            {
                var validNumber = false;
                var firstChar = false;

                switch (c)
                {
                    case '(':
                    case '[':
                        opened = true;
                        firstChar = true;
                        break;

                    case ')':
                        opened = false;
                        var text = builder.ToString();

                        validNumber = int.TryParse(text, out int result);
                        if (validNumber)
                        {
                            footerNumberList.Add(text);
                        }

                        builder = new StringBuilder();
                        break;

                    case ']':
                        opened = false;
                        var builderText = builder.ToString();

                        validNumber = int.TryParse(builderText, out int parsedResult);
                        if (validNumber)
                        {
                            verseNumberList.Add(builderText);
                        }

                        builder = new StringBuilder();
                        break;
                    default:
                        break;
                }

                if (opened && !firstChar)
                {
                    builder.Append(c.ToString());
                }
            }

            var response = passage;
            foreach (var footnoteTag in footerNumberList)
            {
                response = response.Replace(string.Format("({0})", footnoteTag), "");
            }

            foreach (var verseNumberText in verseNumberList)
            {
                string superscript = new string(verseNumberText.Select(i => SuperscriptDigits[i - '0']).ToArray());

                // replace the numbers here with the uincode strings we found above, but add a space at the end
                response = response.Replace(string.Format("[{0}] ", verseNumberText), superscript + " ");
            }

            return response;
        }
    }
}
