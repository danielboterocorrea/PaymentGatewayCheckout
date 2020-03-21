namespace PaymentGateway.Domain.Repositories
{
    public interface IGatewayCache
    {
        bool TryGet<T>(string key, out T value);
        void Remove(string key);
        void UpdateOrCreate<T>(string key, T value, int timePersistenceSeconds = 300);
    }
}
