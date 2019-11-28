using System;
using System.Collections.Generic;
using Diit.CodeMetrics.Data;
using Diit.CodeMetrics.Data.Source;
using Microsoft.Extensions.Logging;

namespace Diit.CodeMetrics.Services
{
    public class MetricsCreator : IMetricsCreator<IMetrics>
    {
        private readonly IMetricsCreator<IMcCeibMetrics> _mcCeibCreator;
        private readonly IMetricsCreator<IHalstedMetrics> _halstedCreator;
        private readonly ILogger<MetricsCreator> _logger;
        private readonly IMetricsCreator<IGilbMetrics> _gilbCreator;


        public MetricsCreator(IMetricsCreator<IMcCeibMetrics> mcCeibCreator,
            IMetricsCreator<IHalstedMetrics> halstedCreator,
            IMetricsCreator<IGilbMetrics> gilbCreator,
            ILogger<MetricsCreator> logger)
        {
            _mcCeibCreator = mcCeibCreator;
            _halstedCreator = halstedCreator;
            _gilbCreator = gilbCreator;
            _logger = logger;
        }

        public IMetrics CreateMetrics(List<Module> source)
        {
            IMetrics metrics = null;
            try
            {
                metrics = (IMetrics) _mcCeibCreator.CreateMetrics(source);

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception in McCeib Metrics");
            }

            try
            {
                IHalstedMetrics mcMetrics = _halstedCreator.CreateMetrics(source);
                if(metrics != null)
                    metrics.HMetrics = mcMetrics.HMetrics;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception in Halsted Metrics");
            }

            try
            {
                IGilbMetrics mcMetrics = _gilbCreator.CreateMetrics(source);
                if (metrics != null)
                    metrics.GMetrics = mcMetrics.GMetrics;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception in Gilb Metrics");
            }
            return metrics ?? new Metrics();
        }
    }
}