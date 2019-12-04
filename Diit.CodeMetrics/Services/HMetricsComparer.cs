using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Diit.CodeMetrics.Data;

namespace Diit.CodeMetrics.Services
{
    public class HMetricsComparer : IHMetricsComparer
    {
        public CompareHMetricsVM CompareTwoProjects(ProjectEntity project1, ProjectEntity project2)
        {
            CompareHMetricsVM compare = new CompareHMetricsVM();
            compare.BetterMetricsCount = 0;

            var dict1 = project1.Metrics.HMetrics;
            var dict2 = project2.Metrics.HMetrics;

            double N1, N2;
            dict1.TryGetValue("Длина программы", out N1);
            dict2.TryGetValue("Длина программы", out N2);
            double result = Math.Round(N2 / N1, 2);
            compare.HMetricsCompareResult.TryAdd("Длина программы", MessageCreate(result, false, compare));

            double V1, V2;
            dict1.TryGetValue("Объем программы", out V1);
            dict2.TryGetValue("Объем программы", out V2);
            result = Math.Round(V2 / V1, 2);
            compare.HMetricsCompareResult.TryAdd("Объем программы", MessageCreate(result, false, compare));

            double _V1, _V2;
            dict1.TryGetValue("Теоретический объем программы", out _V1);
            dict2.TryGetValue("Теоретический объем программы", out _V2);
            result = Math.Round(_V2 / _V1, 2);
            compare.HMetricsCompareResult.TryAdd("Теоретический объем программы", MessageCreate(result, false, compare));

            double _L1, _L2;
            dict1.TryGetValue("Уровень качества программы", out _L1);
            dict2.TryGetValue("Уровень качества программы", out _L2);
            result = Math.Round(_L2 / _L1, 2);
            compare.HMetricsCompareResult.TryAdd("Уровень качества программы", MessageCreate(result, true, compare));

            double L1, L2;
            dict1.TryGetValue("Уровень качества программирования", out L1);
            dict2.TryGetValue("Уровень качества программирования", out L2);
            result = Math.Round(L2 / L1, 2);
            compare.HMetricsCompareResult.TryAdd("Уровень качества программирования", MessageCreate(result, true, compare));

            double Ec1, Ec2;
            dict1.TryGetValue("Сложность понимания программы", out Ec1);
            dict2.TryGetValue("Сложность понимания программы", out Ec2);
            result = Math.Round(Ec2 / Ec1, 2);
            compare.HMetricsCompareResult.TryAdd("Сложность понимания программы", MessageCreate(result, false, compare));

            double D1, D2;
            dict1.TryGetValue("Трудоемкость кодирования программы", out D1);
            dict2.TryGetValue("Трудоемкость кодирования программы", out D2);
            result = Math.Round(D2 / D1, 2);
            compare.HMetricsCompareResult.TryAdd("Трудоемкость кодирования программы", MessageCreate(result, false, compare));

            double I1, I2;
            dict1.TryGetValue("Информационная ёмкость программы", out I1);
            dict2.TryGetValue("Информационная ёмкость программы", out I2);
            result = Math.Round(I2 / I1, 2);
            compare.HMetricsCompareResult.TryAdd("Информационная ёмкость программы", MessageCreate(result, false, compare));

            double E1, E2;
            dict1.TryGetValue("Оценка необходимости интеллектуальных усилий", out E1);
            dict2.TryGetValue("Оценка необходимости интеллектуальных усилий", out E2);
            result = Math.Round(E2 / E1, 2);
            compare.HMetricsCompareResult.TryAdd("Оценка необходимости интеллектуальных усилий", MessageCreate(result, false, compare));

            return compare;
        }

        private string MessageCreate(double result, bool bigger, CompareHMetricsVM compare)
        {
            string message;

            if ((result < 1 && bigger == false) || (result > 1 && bigger == true))
            {
                message = $"ЛУЧШЕ в {result} раз.";
                compare.BetterMetricsCount++;
            }
            else
                message = $"ХУЖЕ в {result} раз.";

            return message;
        }
    }
}
