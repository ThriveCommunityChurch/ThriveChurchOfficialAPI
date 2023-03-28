using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThriveChurchOfficialAPI.Core
{
    public class SermonStatsChartData
    {
        /// <summary>
        /// The date in which the corresponding data is generated from
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The aggregated values 
        /// </summary>
        public double? Value { get; set; }
    }
}