using PaymentGateway.Domain.Model;
using PaymentGateway.Domain.Repositories;
using PaymentGateway.Domain.Validators;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PaymentGateway.Application.Specifications
{
    public class CurrencyExists : ICurrencyExistRule
    {
        private readonly ICurrencyRepository _currencyRepository;

        public CurrencyExists(ICurrencyRepository currencyRepository)
        {
            _currencyRepository = currencyRepository;
        }

        public async Task<(bool isValid, string error, Currency currency)> IsValidAsync(string name)
        {
            var currency = await _currencyRepository.GetByAsync(name);

            if (currency == null)
                return(false,$"Currency {name} was not found", null);

            return (true, string.Empty, currency);
        }
    }
}
