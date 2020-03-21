using System;

namespace PaymentGateway.Domain.Metrics
{
    public interface IMetricsTime
    {
        void RecordTime(string key, TimeSpan time);
    }
}
