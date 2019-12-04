using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Diit.CodeMetrics.Data
{
    public class CompareHMetricsVM
    {
        public Dictionary<string, string> HMetricsCompareResult { get; set; }

        public int BetterMetricsCount { get; set; }

        public CompareHMetricsVM()
        {
            HMetricsCompareResult = new Dictionary<string, string>();
        }
    }
}
