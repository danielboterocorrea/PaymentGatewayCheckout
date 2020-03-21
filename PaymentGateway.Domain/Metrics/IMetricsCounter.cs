namespace PaymentGateway.Domain.Metrics
{
    public interface IMetricsCounter
    {
        void IncrementCounter(string key);
    }
}
