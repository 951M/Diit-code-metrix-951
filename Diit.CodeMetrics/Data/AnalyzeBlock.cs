using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Diit.CodeMetrics.Data
{
    public class AnalyzerBlock
    {
        public int LineNumber { get; set; }
        public bool IsBranchStart { get; set; } = false;
        public BranchType BranchType { get; set; } = BranchType.None;
        public bool IsStart { get; set; } = false;
        public List<int> ConnectedToLine { get; set; } = new List<int>();
        public List<int> ConnectedFromLine { get; set; } = new List<int>();
    }
}
