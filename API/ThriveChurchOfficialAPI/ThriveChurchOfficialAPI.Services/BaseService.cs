using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI
{
    public abstract class BaseService
    {

        public string GetBetween(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);

                if (End == -1 || Start == -1)
                {
                    return "";
                }

                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "";
            }
        }

        public PassagesResponse ConvertESVTextIntoConsumibleObjects(PassageTextInfo getPassagesResponse)
        {
            var passage = getPassagesResponse.passages.ToList()[0];
            var canonical = getPassagesResponse.canonical;
            var passages = GetBetween(passage, canonical, "Footnotes");
            var footnotes = GetBetween(passage, "Footnotes", "(ESV)");

            // we now need to split them both and return a list of each 
            var passageList = passages.Split('[');
            var footnoteList = footnotes.Split('(').ToList();

            // we will need to store these objects in a list so we can return them easily
            var passagesList = new List<Passage>();
            var footnotesList = new List<Footnote>();

            foreach (var psg in passageList)
            {
                // we will need to get the verse # from the beginning of this string
                // however the verse number might be 1-3 digits so we need to split by ]
                var passageSplit = psg.Split(']');
                var verseNumberText = passageSplit[0];
                var passageNumber = Int32.TryParse(verseNumberText, out int x);
                bool isHeader = false;

                // the text looks a bit dirty, clean it up
                var passageText = verseNumberText.Replace("\n", "").Trim();
                var headerText = GetBetween(psg, "\n\n", "\n\n  ");

                // try again because they might be being cheeky 
                if (string.IsNullOrEmpty(headerText))
                {
                    headerText = GetBetween(psg, "\n    \n    \n    ", "\n\n");

                    if (!string.IsNullOrEmpty(headerText))
                    {
                        isHeader = true;
                    }
                }

                var footnoteNumber = GetBetween(psg, "(", ")");

                if (passageSplit.Length > 1)
                {
                    if (footnoteNumber != "")
                    {
                        var stringToReplace = string.Format("({0})", footnoteNumber);
                        passageSplit[1] = passageSplit[1].Replace(stringToReplace, "");
                    }
                    
                    // it is possible that a verse will randomly have a header in the middle of it
                    string SuperscriptDigits = "\u2070\u00b9\u00b2\u00b3\u2074\u2075\u2076\u2077\u2078\u2079";
                    string superscript = new string(verseNumberText.Select(i => SuperscriptDigits[i - '0']).ToArray());

                    if (isHeader)
                    {
                        // do something because we will want to preserve the linebreaks but we don't want them in the middle of the verse text
                        // or should we do something else???????
                        passageText = passageSplit[1].Trim();
                    }
                    else
                    {
                        passageSplit[1] = superscript + passageSplit[1].Replace("\n", "").Trim();
                        passageText = passageSplit[1];
                    }
                }
                else
                {
                    // it's likely that a verse number of 0 will always be a header, this might be an edge case -- will have to verify this
                    // but it looks to be unimportant
                    isHeader = true;
                }

                var passageToAdd = new Passage()
                {
                    Verse = isHeader == false ? passageText : headerText, // we can update this at once with this nice syntax
                    VerseNumber = x,
                    IsHeader = isHeader
                };
                passagesList.Add(passageToAdd);
            }

            if (footnoteList.Any())
            {
                // skip the first one
                footnoteList.RemoveAt(0);
            }

            foreach (var footnote in footnoteList)
            {
                // we will need to get the verse # from the beginning of this string
                // however the verse number might be 1-3 digits so we need to split by ]
                var footnoteSplit = footnote.Split(')');

                var footnoteText = footnoteSplit[1].Replace("\n", "");

                // Determine the chapter and verse of the footnote
                var footnoteInfo = footnoteText.Split(':');
                var footnoteChapter = footnoteInfo[0];
                var verseInfo = footnoteInfo[1].Split(' ');

                var isNumVerse = Int32.TryParse(verseInfo[0], out int x);
                var isNumChapter = Int32.TryParse(footnoteChapter, out int y);

                var stringToRemove = string.Format("{0}:{1} ", y, x);
                var text = footnoteText.Replace(stringToRemove, "").Trim();

                var footnoteToAdd = new Footnote()
                {
                    FootnoteInfo = text,
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
