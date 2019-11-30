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
    public class CommentMetricsCreator : IMetricsCreator<ICommentMetrics>
    {

        private readonly ILexicalAnalyzer<ICommentMetrics> _lexicalAnalyzer;

        public CommentMetricsCreator(ILexicalAnalyzer<ICommentMetrics> lexicalAnalyzer)
        {
            _lexicalAnalyzer = lexicalAnalyzer;
        }


        public ICommentMetrics CreateMetrics(List<Data.Source.Module> source)
        {
            ICommentMetrics metrics = new Metrics();
            string allsource = " ";
            foreach (var i in source)
            {
                allsource += Encoding.UTF8.GetString(i.Source);
            }
            var analized = _lexicalAnalyzer.AnalyzeSource(allsource).FirstOrDefault();
            metrics.CMetrics = Calculate(analized.CommentCounter, analized.OperatorsCounter, analized.LineNumber,
                analized.OperatorsBlockCounter, analized.CommentBlockCounter);
            return metrics;
        }

        private Dictionary<string, double> Calculate(double Ncomments, double Moperators, double Mlines,
            List<int> operatorsBlockCounter, List<int> commentBlockCounter)
        {
            Dictionary<string, double> metriks = new Dictionary<string, double>();
            try
            {
                double f1 = 0, f2 = 0, f3 = 0, f4 = 0;

                f1 = Ncomments / Mlines;

                if (Moperators == 0)
                {
                    f1 = 0; f4 = 0;
                }
                else
                {
                    f2 = Ncomments / Moperators;

                    for (int i = 0; i < commentBlockCounter.Count; ++i)
                    {
                        if (operatorsBlockCounter[i] == 0)
                            operatorsBlockCounter[i] = 1;

                        f4 += Math.Sign(((double)commentBlockCounter[i] / ((double)operatorsBlockCounter[i]))-0.1);
                    }
                }

                for(int i = 0; i < commentBlockCounter.Count; ++i)
                {
                    f3 += Math.Sign(((double)commentBlockCounter[i] / ((double)Mlines / (double)commentBlockCounter.Count))-0.1);
                }

                metriks.TryAdd("Колличество комментариев", Ncomments);
                metriks.TryAdd("Колличество блоков", commentBlockCounter.Count);
                metriks.TryAdd("Комментированность общая через строки", f1);
                metriks.TryAdd("Комментированность общая через операторы", f2);
                metriks.TryAdd("Комментированность блочная через строки", f3);
                metriks.TryAdd("Комментированность блочная через операторы", f4);

                return metriks;
            }
            catch
            {
                //("Error: " + ex);
                return null;
            }
        }
    }
}