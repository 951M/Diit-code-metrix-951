using Diit.CodeMetrics.Data;
using Diit.CodeMetrics.Data.Source;
using Diit.CodeMetrics.Services.Analyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Diit.CodeMetrics.Services
{
    public class ChepinaMetricsCreator : IMetricsCreator<IChepinaMetrics>
    {
        private float a1 = 1;
        private float a2 = 2;
        private float a3 = 3;
        private float a4 = 0.5f;

        private readonly ILexicalAnalyzer<IChepinaMetrics> _lexicalAnalyzer;

        public ChepinaMetricsCreator(ILexicalAnalyzer<IChepinaMetrics> lexicalAnalyzer)
        {
            _lexicalAnalyzer = lexicalAnalyzer;
        }


        public IChepinaMetrics CreateMetrics(List<Data.Source.Module> source)
        {
            IChepinaMetrics metrics = new Metrics();
            string allsource = " ";
            foreach (var i in source)
            {
                allsource += Encoding.UTF8.GetString(i.Source);
            }
            var analized = _lexicalAnalyzer.AnalyzeSource(allsource).FirstOrDefault();
            metrics.ChMetrics = Calculate(analized.CommentCounter, (int)analized.OperatorsCounter,
                analized.LineNumber, (int)analized.A_coef);
            return metrics;
        }

        private Dictionary<string, double> Calculate(int nUnUsedVariables, int nUsedVariables, int nConfigVariables,
            int nReadOnlyVariables)
        {
            Dictionary<string, double> metriks = new Dictionary<string, double>();
            try
            {
                float result = a1* nReadOnlyVariables + a2* nUsedVariables + a3* nConfigVariables + a4* nUnUsedVariables;



                metriks.TryAdd("Количество множество Р", nReadOnlyVariables);
                metriks.TryAdd("Количество множество M", nUsedVariables);
                metriks.TryAdd("Количество множество C", nConfigVariables);
                metriks.TryAdd("Количество множество T", nUnUsedVariables);
                metriks.TryAdd("значение метрики Чепина:", result);


                return metriks;
            }
            catch
            {
                return null;
            }
        }
    }
}