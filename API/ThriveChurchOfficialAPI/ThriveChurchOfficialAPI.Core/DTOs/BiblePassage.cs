using System;
using System.Collections.Generic;
using System.Text;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Bible passages referencing parts of scripture
    /// </summary>
    public class BiblePassage: ObjectBase
    {
        /// <summary>
        /// C'tor
        /// </summary>
        public BiblePassage()
        {
            PassageRef = null;
            PassageText = null;
        }

        /// <summary>
        /// Section of scripture that this passage coorisponds to
        /// </summary>
        public string PassageRef { get; set; }

        /// <summary>
        /// Preformatted passage text
        /// </summary>
        public string PassageText { get; set; }
    }
}
