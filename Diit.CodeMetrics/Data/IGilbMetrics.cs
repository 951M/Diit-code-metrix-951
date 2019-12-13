using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Diit.CodeMetrics.Data
{
    public interface IGilbMetrics : IBaseMetrics
    {
        Dictionary<string, double> GMetrics { get; set; }
    }
}
