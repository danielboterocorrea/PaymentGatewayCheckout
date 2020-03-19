using PaymentGateway.Domain.Model;
using PaymentGateway.Domain.Repositories;
using PaymentGateway.Domain.Validators;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PaymentGateway.Application.Specifications
{
    public class MerchantExists : IMerchantExistRule
    {
        private readonly IMerchantRepository _merchantRepository;

        public MerchantExists(IMerchantRepository merchantRepository)
        {
            _merchantRepository = merchantRepository;
        }
        public async Task<(bool isValid,string error, Merchant merchant)> IsValidAsync(string name)
        {
            var merchant = await _merchantRepository.GetByAsync(name);
            if (merchant == null)
                return (false, $"Merchant {name} was not found", null);

            return (true, string.Empty, merchant);
        }
    }
}
