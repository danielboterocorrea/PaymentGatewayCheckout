using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentGateway.Domain.Metrics
{
    public interface IMetricsTime
    {
        void RecordTime(string key, TimeSpan time);
    }
}
