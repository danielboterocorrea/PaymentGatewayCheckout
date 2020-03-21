using PaymentGateway.Domain.Metrics;

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
