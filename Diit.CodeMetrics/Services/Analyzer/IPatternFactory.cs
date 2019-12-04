using Diit.CodeMetrics.Data;
using System.Text.RegularExpressions;

namespace Diit.CodeMetrics.Services.Analyzer
{
    public interface IPatternFactory
    {
        Regex GetPattern(PatterTypesEnum patternType);
    }
}