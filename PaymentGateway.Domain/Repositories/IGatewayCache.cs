using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentGateway.Domain.Repositories
{
    public interface IGatewayCache
    {
        bool TryGet<T>(string key, out T value);
        void Remove(string key);
        void UpdateOrCreate<T>(string key, T value);
    }
}
