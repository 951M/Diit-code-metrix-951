using Diit.CodeMetrics.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Diit.CodeMetrics.Services.Analyzer
{
    public class GilbSharpAnalyzer : ILexicalAnalyzer<IGilbMetrics>
    {
        private readonly IPatternFactory _patternFactory;
        public GilbSharpAnalyzer(IPatternFactory patternFactory)
        {
            _patternFactory = patternFactory;
        }
        public IEnumerable<AnalyzerItem> AnalyzeSource(string source)
        {
            var patternForBlocks = _patternFactory.GetPattern(PatterTypesEnum.BlocksToDelete);
            source = patternForBlocks.Replace(source, " ");

            var patternForLines = _patternFactory.GetPattern(PatterTypesEnum.LinesToDelete);
            source = patternForLines.Replace(source, " ");

            var patternForDividers = _patternFactory.GetPattern(PatterTypesEnum.DividersPattern);
            source = patternForDividers.Replace(source, " ");

            var lexems = source.Split(" ").ToList();
            lexems.RemoveAll(lexem =>
                string.IsNullOrWhiteSpace(lexem));

            var conditionOperators = 0;
            var allOperators = 0;

            var patternForAllOperators = _patternFactory.GetPattern(PatterTypesEnum.OperatorsPattern);
            var patternForConditionOperators = _patternFactory.GetPattern(PatterTypesEnum.ConditionOperatorsPatter);
            var patternForComments = _patternFactory.GetPattern(PatterTypesEnum.Comments);
            foreach (var lexem in lexems)
            {
                if (patternForConditionOperators.IsMatch(lexem))
                {
                    conditionOperators++;
                    allOperators++;
                }
                else if (patternForAllOperators.IsMatch(lexem))
                {
                    allOperators++;
                }
            }

            return new[]
            {
                new AnalyzerItem
                {
                    AllOperatorsCount = allOperators,
                    ConditionOperatorsCount = conditionOperators
                }
            };
        }
    }
}
