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
                analized.OperatorsBlockCounter, analized.CommentBlockCounter, analized.Isprime, analized.A_coef);
            return metrics;
        }

        private Dictionary<string, double> Calculate(double Ncomments, double Moperators, double Mlines,
            List<int> operatorsBlockCounter, List<int> commentBlockCounter, bool isprime, double a_coef)
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

                        if (!isprime)
                            f4 += Math.Sign(((double)commentBlockCounter[i] / ((double)operatorsBlockCounter[i])) - 0.1 - a_coef);
                        else
                        {
                            if(i == commentBlockCounter.Count - 1)
                                f4 += Math.Sign(((double)commentBlockCounter[i] / ((double)operatorsBlockCounter[i])) - 0.1 - a_coef);
                            else
                                f4 += Math.Sign(((double)commentBlockCounter[i] / ((double)operatorsBlockCounter[i])) - 0.1);
                        }
                    }
                }

                for(int i = 0; i < commentBlockCounter.Count; ++i)
                {
                    if (!isprime)
                        f3 += Math.Sign(((double)commentBlockCounter[i] / ((double)Mlines / (double)commentBlockCounter.Count)) - 0.1 - a_coef);
                    else
                    {
                        if (i == commentBlockCounter.Count - 1)
                            f3 += Math.Sign(((double)commentBlockCounter[i] / ((double)Mlines / (double)commentBlockCounter.Count)) - 0.1 - a_coef);
                        else
                            f3 += Math.Sign(((double)commentBlockCounter[i] / ((double)Mlines / (double)commentBlockCounter.Count)) - 0.1);
                    }
                }



                metriks.TryAdd("Колличество комментариев", Ncomments);
                metriks.TryAdd("Колличество блоков", commentBlockCounter.Count);

                string analysLines = "Недостаточно";

                if (f1 > 0.1f)
                    analysLines = "Достаточно";

                metriks.TryAdd("Комментированность общая через строки(" + analysLines + ")", Math.Round(f1, 4));

                analysLines = "Недостаточно";
                if (f2 > 0.1f)
                    analysLines = "Достаточно";

                metriks.TryAdd("Комментированность общая через операторы(" + analysLines + ")", Math.Round(f2, 4));

                string analysBlock = "Недостаточно";
                if (f3 == commentBlockCounter.Count)
                    analysBlock = "Достаточно";

                metriks.TryAdd("Комментированность блочная через строки(" + analysBlock + ")", f3);

                analysBlock = "Недостаточно";
                if (f4 == commentBlockCounter.Count)
                    analysBlock = "Достаточно";

                metriks.TryAdd("Комментированность блочная через операторы(" + analysBlock + ")", f4);

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