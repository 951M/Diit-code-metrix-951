using System.Collections.Generic;
using Diit.CodeMetrics.Services.Analyzer;

namespace Diit.CodeMetrics.Data
{
    public class AnalyzerItem
    {
        // McCabe
        public bool? IsProcedure { get; set; }
        public int LineNumber { get; set; }
        public BranchType BranchType { get; set; }
        public List<int> ConnectedFrom { get; set; }  = new List<int>();
        public string Comment { get; set; }
        // Halsted
        public double UniqueOperators { get; set; }
        public double UniqueOperands { get; set; }
        public double OperatorsCounter { get; set; }
        public double OperandsCounter { get; set; }
        public double TeoryOperators { get; set; }
        public double TeoryOperands { get; set; }
        // Gilb
        public int ConditionOperatorsCount { get; set; }
        public int AllOperatorsCount { get; set; }
    }
}