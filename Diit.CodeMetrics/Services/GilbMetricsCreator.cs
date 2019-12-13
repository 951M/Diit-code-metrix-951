using Diit.CodeMetrics.Data;
using Diit.CodeMetrics.Data.Source;
using Diit.CodeMetrics.Services.Analyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diit.CodeMetrics.Services
{
    public class GilbMetricsCreator : IMetricsCreator<IGilbMetrics>
    {
        private readonly ILexicalAnalyzer<IGilbMetrics> _lexicalAnalyzer;

        public GilbMetricsCreator(ILexicalAnalyzer<IGilbMetrics> lexicalAnalyzer)
        {
            _lexicalAnalyzer = lexicalAnalyzer;
        }

        public IGilbMetrics CreateMetrics(List<Module> source)
        {
            var sourceData = string.Empty;
            foreach (var module in source)
                sourceData += Encoding.UTF8.GetString(module.Source);

            var parsedData = _lexicalAnalyzer.AnalyzeSource(sourceData).FirstOrDefault();
            var gilbProgrammComplexity =
                Math.Round((double)parsedData.ConditionOperatorsCount / parsedData.AllOperatorsCount, 2);

            var analaysResult = "относительно простая";

            if (gilbProgrammComplexity > 0.2f)
                analaysResult = "относительно сложная";

            var resultDictionary = new Dictionary<string, double>
            {
                { "Число условных операторов", parsedData.ConditionOperatorsCount},
                { "Число всех операторов", parsedData.AllOperatorsCount},
                { "Сложность программы по Джидбу - " + analaysResult +
                    " в виду того, что отношение кол-ва условных " +
                    "операторов к кол-ву всех операторов", gilbProgrammComplexity}
            };


            return new Metrics
            {
                GMetrics = resultDictionary
            };
        }
    }
}
