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
        public static string messageFormat = "Merchant {0} was not found";

        public MerchantExists(IMerchantRepository merchantRepository)
        {
            _merchantRepository = merchantRepository;
        }
        public async Task<(bool isValid,string error, Merchant merchant)> IsValidAsync(string name)
        {
            var merchant = await _merchantRepository.GetByAsync(name);
            if (merchant == null)
                return (false, string.Format(messageFormat, name), null);

            return (true, string.Empty, merchant);
        }
    }
}
