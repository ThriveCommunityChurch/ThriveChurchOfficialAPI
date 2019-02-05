using System.Collections.Generic;

namespace ThriveChurchOfficialAPI
{
    public class PassageMetadata
    {
        public PassageMetadata()
        {
            chapter_start = null;
            chapter_end = null;
            next_chapter = null;
            prev_chapter = null;
        }

        public string canonical { get; set; }

        public IEnumerable<int> chapter_start { get; set; }

        public IEnumerable<int> chapter_end { get; set; }

        public int? prev_verse { get; set; }

        public int? next_verse { get; set; }

        public IEnumerable<int> prev_chapter { get; set; }

        public IEnumerable<int> next_chapter { get; set; }
    }
}