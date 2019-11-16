using System.Collections.Generic;
using Diit.CodeMetrics.Data;
using Diit.CodeMetrics.Data.Source;

namespace Diit.CodeMetrics.Services.Analyzer
{
    public interface ILexicalAnalyzer<T> where T: IBaseMetrics
    {
        //        int IsLineProcedure(string line);
        //        int GetBranchCountInLine(string line);
        IEnumerable<AnalyzerItem> AnalyzeSource(string source);
    }
}