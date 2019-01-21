using System;
using System.Collections.Generic;
using System.Text;

namespace ThriveChurchOfficialAPI.Core
{
    public class PageInfo
    {
        public PageInfo()
        {
            PageNumber = 0;
            TotalPageCount = 0;
            TotalRecordCount = 0;
        }

        /// <summary>
        /// The current selected page
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Total number of pages that are available to request
        /// </summary>
        public int TotalPageCount { get; set; }

        /// <summary>
        /// The total count of records that can be returned within pages
        /// </summary>
        public int TotalRecordCount { get; set; }
    }
}