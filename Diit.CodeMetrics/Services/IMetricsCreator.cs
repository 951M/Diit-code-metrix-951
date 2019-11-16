using System.Collections.Generic;
using Diit.CodeMetrics.Data;
using Diit.CodeMetrics.Data.Source;

namespace Diit.CodeMetrics.Services
{
    public interface IMetricsCreator <out T> where T : IBaseMetrics
    {
        T CreateMetrics(List<Module> source);
    }
}