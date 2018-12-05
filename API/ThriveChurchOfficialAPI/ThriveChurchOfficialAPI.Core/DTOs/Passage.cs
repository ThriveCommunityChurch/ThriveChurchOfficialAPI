using System.Collections.Generic;

namespace ThriveChurchOfficialAPI
{
    public class Passage
    {
        public Passage()
        {
            IsHeader = false;
        }

        public int VerseNumber { get; set; }

        public string Verse { get; set; }

        public bool IsHeader { get; set; }
    }
}