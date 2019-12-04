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
    public class HalstedMetricsCreator : IMetricsCreator<IHalstedMetrics>
    {

        private readonly ILexicalAnalyzer<IHalstedMetrics> _lexicalAnalyzer;

        public HalstedMetricsCreator(ILexicalAnalyzer<IHalstedMetrics> lexicalAnalyzer)
        {
            _lexicalAnalyzer = lexicalAnalyzer;
        }


        public IHalstedMetrics CreateMetrics(List<Data.Source.Module> source)
        {
            IHalstedMetrics metrics = new Metrics();
            string allsource = " ";
            foreach (var i in source)
            {
                allsource += Encoding.UTF8.GetString(i.Source);
            }
            var analized = _lexicalAnalyzer.AnalyzeSource(allsource).FirstOrDefault();
            metrics.HMetrics = Calculate(analized.UniqueOperators, analized.UniqueOperands, analized.OperatorsCounter, analized.OperandsCounter, analized.TeoryOperators, analized.TeoryOperands);
            return metrics;
        }

        private Dictionary<string, double> Calculate(double n1, double n2, double N1, double N2, double _n1, double _n2)
        {
            Dictionary<string, double> metriks = new Dictionary<string, double>();
            try
            {
                double n = n1 + n2;                                        	//словарь программы
                metriks.TryAdd("Словарь программы", n);

                double N = N1 + N2;                                        	//длина программы
                metriks.TryAdd("Длина программы", N);

                double _n = (n1 * Math.Log(n1, 2)) + (n2 * Math.Log(n2, 2));   //теоретический словарь программы
                metriks.TryAdd("Теоретический словарь программы", _n);

                double _N = _n1 + _n2;                                     	//теоретическая длина программы
                metriks.TryAdd("Теоретическая длина программы", _N);

                double V = N * Math.Log(n, 2);                             	//обьем программы
                metriks.TryAdd("Объем программы", V);

                double _V = _N * Math.Log(_n, 2);                          	//теоретический обьем программы
                metriks.TryAdd("Теоретический объем программы", _V);

                double _L = V / _V;                                        	//уровень качества программы в идеале 1
                metriks.TryAdd("Уровень качества программы", _L);

                double L = 2 * n2 / (n1 * N2);                    	        //уровень качества программирования
                metriks.TryAdd("Уровень качества программирования", L);

                double Ec = V / Math.Pow(_L, 2);                           	//сложность понимания программы
                metriks.TryAdd("Сложность понимания программы", Ec);

                double D = 1 / _L;                                         	//трудоемкость кодирования программы
                metriks.TryAdd("Трудоемкость кодирования программы", D);

                double I = V / D;                                          	//информационная емкость программы
                metriks.TryAdd("Информационная ёмкость программы", I);

                /*оценка необходимости интеллектуальных усилий 
                при разработке программы*/
                double E = _N * Math.Log(n / L);
                metriks.TryAdd("Оценка необходимости интеллектуальных усилий", E);

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