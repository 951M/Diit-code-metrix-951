using System.Collections.Generic;

namespace Diit.CodeMetrics.Data
{
    public class Metrics : IMetrics
    {
        public Dictionary<string, List<GraphEntity>> GraphEntities { get; set; }
        public int ComplexityNumber { get; set; }
        public Dictionary<string,double> HMetrics { get; set; }
        public Dictionary<string, double> GMetrics { get; set; }
    }
}