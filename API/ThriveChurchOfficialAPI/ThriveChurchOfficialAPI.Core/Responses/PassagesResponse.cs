using System;
using System.Collections.Generic;
using System.Text;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// C'tor
    /// </summary>
    public class PassagesResponse
    {
        public PassagesResponse()
        {
            Passages = null;
            Footnotes = null;
        }

        public IEnumerable<Passage> Passages { get; set; }

        public string Canonical { get; set; }

        public IEnumerable<Footnote>  Footnotes { get; set; }
    }
 }
