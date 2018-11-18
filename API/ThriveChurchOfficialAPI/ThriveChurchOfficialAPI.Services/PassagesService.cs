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

            searchCriteria = "John 1";

            // since ESV returns everything as one massive string, I need to convert everything to objects
            // Then to strings if I wish
            var getPassagesResponse = await _passagesRepository.GetPassagesForSearch(EsvApiKey, searchCriteria);

            var passage = getPassagesResponse.passages.ToList()[0];
            var canonical = getPassagesResponse.canonical;
            var passages = GetBetween(passage, canonical, "Footnotes");
            var footnotes = GetBetween(passage, "Footnotes", "(ESV)");

            // we now need to split them both and return a list of each 
            var passageList = passages.Split('[');
            var footnoteList = footnotes.Split('(');

            // we will need to store these objects in a list so we can return them easily
            var passagesList = new List<Passage>();
            var footnotesList = new List<Footnote>();

            foreach (var psg in passageList)
            {
                // we will need to get the verse # from the beginning of this string
                // however the verse number might be 1-3 digits so we need to split by ]
                var passageSplit = psg.Split(']');
                var passageNumber = Int32.TryParse(passageSplit[0], out int x);

                // we will still neet to parse out the titles to all the stories somehow........
                var passageText = passageSplit[0].Replace("\n", "").Trim();
                var headerText = GetBetween(psg, "\n\n", "\n\n  ");

                if (passageSplit.Length > 1)
                {
                    passageText = passageSplit[1].Replace("\n", "").Trim();

                    if (passageText == headerText)
                    {
                        // do something 
                    }
                }
                else
                {
                    if (passageText == headerText)
                    {
                        // do something 
                        // add the string to a IEnumerable<Header> object with a reference to which verse
                        // this header appears before
                        // Otherwise we don't modify the strings at all and just let them be
                        // maybe viewing https://api.esv.org/docs/passage-text/ will help much of this headache
                    }
                }

                var passageToAdd = new Passage()
                {
                    Verse = passageText,
                    VerseNumber = x
                };
                passagesList.Add(passageToAdd);
            }

            foreach (var footnote in footnoteList)
            {
                // skip the first one
                if (footnote == "\n\n")
                {
                    continue;
                }

                // we will need to get the verse # from the beginning of this string
                // however the verse number might be 1-3 digits so we need to split by ]
                var footnoteSplit = footnote.Split(')');

                var footnoteText = footnoteSplit[1].Replace("\n", "");

                // Determine the chapter and verse of the footnote
                var footnoteInfo = footnoteText.Split(':');
                var footnoteChapter = footnoteInfo[0];
                var verseInfo = footnoteInfo[1].Split(' ');
                // it might actually look super cool to make the verse numbers superscripted above, so we will need to keep them, removing the [] around it
                // and then converting it to a unicode superscripted number like so:
                // 
                // USE THIS EXAMPLE
                //
                //string SuperscriptDigits =  "\u2070\u00b9\u00b2\u00b3\u2074\u2075\u2076\u2077\u2078\u2079";
                //var text = "25432";
                //string superscript = new string(text.Select(x => SuperscriptDigits[x - '0']).ToArray());
                // 
            
                var isNumVerse = Int32.TryParse(verseInfo[0], out int x);
                var isNumChapter = Int32.TryParse(footnoteChapter, out int y);

                var stringToRemove = string.Format("{0}:{1} ", y, x);
                var text = footnoteText.Replace(stringToRemove, "");

                var footnoteToAdd = new Footnote()
                {
                    FootnoteInfo = text.Trim(),
                    ChapterNumber = y,
                    VerseNumber = x
                };
                footnotesList.Add(footnoteToAdd);
            }

            var response = new PassagesResponse()
            {
                Canonical = canonical,
                Footnotes = footnotesList,
                Passages = passagesList
            };


            return response;
        }
    }
}
