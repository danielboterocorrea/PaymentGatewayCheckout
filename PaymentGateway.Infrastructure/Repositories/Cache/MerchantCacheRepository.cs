using Microsoft.Extensions.Logging;
using PaymentGateway.Domain.Model;
using PaymentGateway.Domain.Repositories;
using System.Threading.Tasks;

namespace PaymentGateway.Infrastructure.Repositories.Cache
{
    public class MerchantCacheRepository : IMerchantRepository
    {
        private readonly MerchantRepository _merchantRepository;
        private readonly ILogger<CurrencyCacheRepository> _logger;
        private readonly IGatewayCache _cache;

        public MerchantCacheRepository(MerchantRepository merchantRepository,
             ILogger<CurrencyCacheRepository> logger,
             IGatewayCache cache)
        {
            _merchantRepository = merchantRepository;
            _logger = logger;
            _cache = cache;
        }

        public async Task<Merchant> GetByAsync(string value)
        {
            string key = "Merchant" + value;
            _logger.LogInformation($"MerchantCacheRepository - GetByAsync({value})");

            var currencyExisted = _cache.TryGet(key, out Merchant merchant);

            if (currencyExisted)
            {
                _logger.LogInformation($"MerchantCacheRepository - Hit Cache with key {key} - GetByAsync({value})");
                return merchant;
            }

            var merchantFromDb = await _merchantRepository.GetByAsync(value);
            _cache.UpdateOrCreate(key, merchantFromDb);

            return merchantFromDb;
        }
    }
}
