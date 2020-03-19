using Microsoft.Extensions.Logging;
using PaymentGateway.Domain.Model;
using PaymentGateway.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PaymentGateway.Infrastructure.Repositories.Cache
{
    public class CurrencyCacheRepository : ICurrencyRepository
    {
        private readonly ILogger<CurrencyCacheRepository> _logger;
        private readonly IGatewayCache _cache;
        private readonly CurrencyRepository _currencyRepository;

        public CurrencyCacheRepository(CurrencyRepository currencyRepository,
            ILogger<CurrencyCacheRepository> logger,
            IGatewayCache cache)
        {
            _currencyRepository = currencyRepository;
            _logger = logger;
            _cache = cache;
        }

        public async Task<Currency> GetByAsync(string value)
        {
            string key = "Currency_" + value;
            _logger.LogInformation($"CurrencyCacheRepository - GetByAsync({value})");

            var currencyExisted = _cache.TryGet(key, out Currency currency);

            if(currencyExisted)
            {
                _logger.LogInformation($"CurrencyCacheRepository - Hit Cache with key {key}  - GetByAsync({value})");
                return currency;
            }

            var currencyFromDb = await _currencyRepository.GetByAsync(value);
            _cache.UpdateOrCreate(key, currencyFromDb);
            return currencyFromDb;
        }
    }
}
