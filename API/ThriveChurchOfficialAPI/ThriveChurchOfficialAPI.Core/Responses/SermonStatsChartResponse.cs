using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThriveChurchOfficialAPI.Core
{
    public class SermonStatsChartResponse
    {
        /// <summary>
        /// A collection of data points used for display in a chart
        /// </summary>
        public IEnumerable<SermonStatsChartData> Data { get; set; }
    }
}
