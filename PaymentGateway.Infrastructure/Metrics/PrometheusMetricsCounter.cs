using PaymentGateway.Domain.Metrics;
using Prometheus;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentGateway.Infrastructure.Metrics
{
    public class PrometheusMetricsCounter : IMetricsCounter
    {
        public void IncrementCounter(string key)
        {
            var counter = Prometheus.Metrics.CreateCounter(key, MetricsCountsData.Descriptions[key]);
            counter.Inc();
        }
    }
}
