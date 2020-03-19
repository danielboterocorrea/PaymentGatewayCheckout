using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PaymentGateway.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentGateway.Infrastructure.Repositories.Cache
{
    public class InMemoryGatewayCache : IGatewayCache
    {
        private static MemoryCache _cache;
        private readonly object _locker = new object();
        private readonly ILogger<InMemoryGatewayCache> _logger;

        public InMemoryGatewayCache(ILogger<InMemoryGatewayCache> logger)
        {
            _logger = logger;
            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        public bool TryGet<T>(string key, out T value)
        {
            lock (_locker)
            {
                _logger.LogDebug("TryGet");
                _logger.LogDebug($"Getting Key: {key}");
                // Look for cache key.
                if (!_cache.TryGetValue(key, out T cacheEntry))
                {
                    _logger.LogDebug($"Key not found: {key}");
                    value = default(T);
                    return false;
                }
                value = cacheEntry;
                _logger.LogDebug($"Key found: {key}, value: {value}");
                return true;
            }
        }

        public void Remove(string key)
        {
            lock (_locker)
            {
                _logger.LogDebug("Remove");
                _logger.LogDebug($"Removing key: {key}");
                _cache.Remove(key);
            }
        }

        public void UpdateOrCreate<T>(string key, T value)
        {
            lock (_locker)
            {
                _logger.LogDebug("UpdateOrCreate");
                _logger.LogDebug($"UpdateOrCreate key: {key}");

                var isOk = TryGet(key, out T _);
                if (isOk)
                {
                    _cache.Remove(key);
                }
                _logger.LogDebug($"Setting key: {key}, value: {value}");

                MemoryCacheEntryOptions cacheExpirationOptions = new MemoryCacheEntryOptions();
                cacheExpirationOptions.AbsoluteExpiration = DateTime.Now.AddMinutes(5);
                cacheExpirationOptions.Priority = CacheItemPriority.Normal;

                _cache.Set(key, value, cacheExpirationOptions);
            }
        }
    }
}
