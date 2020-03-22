using Microsoft.Extensions.Logging;
using PaymentGateway.Domain.Model;
using PaymentGateway.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaymentGateway.Infrastructure.Repositories.Cache
{
    public class PaymentCacheRepository : IPaymentRepository
    {
        private readonly ILogger<PaymentCacheRepository> _logger;
        private readonly IGatewayCache _cache;
        private readonly PaymentRepository _paymentRepository;

        public PaymentCacheRepository(PaymentRepository paymentRepository,
            ILogger<PaymentCacheRepository> logger,
            IGatewayCache cache)
        {
            _paymentRepository = paymentRepository;
            _logger = logger;
            _cache = cache;
        }

        public async Task AddAsync(Payment payment)
        {
            await _paymentRepository.AddAsync(payment);
        }

        public async Task<IList<Payment>> GetAsync()
        {
            return await _paymentRepository.GetAsync();
        }

        public async Task<Payment> GetAsync(Guid id)
        {
            string key = "Payment_" + id;
            _logger.LogInformation($"PaymentCacheRepository - GetByAsync({id})");

            var paymentExisted = _cache.TryGet(key, out Payment paymentCache);

            if (paymentExisted)
            {
                _logger.LogInformation($"PaymentCacheRepository - Hit Cache with key {key}  - GetByAsync({paymentCache.Id})");
                return paymentCache;
            }

            var paymentFromDb = await _paymentRepository.GetAsync(id);
            if(paymentFromDb != null)
                _cache.UpdateOrCreate(key, paymentFromDb, 30);
            return paymentFromDb;
        }

        public async Task UpdateAsync(Payment payment)
        {
            //TODO: Create format, same in all cache repositories
            string key = "Payment_" + payment.Id;
            _cache.Remove(key);
            await _paymentRepository.UpdateAsync(payment);
        }
    }
}
