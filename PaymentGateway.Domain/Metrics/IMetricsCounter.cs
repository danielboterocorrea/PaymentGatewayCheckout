using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentGateway.Domain.Metrics
{
    public interface IMetricsCounter
    {
        void IncrementCounter(string key);
    }
}
