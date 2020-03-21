using PaymentGateway.Domain.Metrics;
using System;
using Prometheus;

namespace PaymentGateway.Infrastructure.Metrics
{
    public class PrometheusMetricsTime : IMetricsTime
    {
        public void RecordTime(string key, TimeSpan time)
        {
            var histogram = Prometheus.Metrics.CreateHistogram(key, MetricsTimeData.Descriptions[key],
                new HistogramConfiguration
                {
                    Buckets = Histogram.LinearBuckets(start: 1, width: 1, count: 5)
                });

            histogram.Observe(time.TotalSeconds);
        }
    }
}
