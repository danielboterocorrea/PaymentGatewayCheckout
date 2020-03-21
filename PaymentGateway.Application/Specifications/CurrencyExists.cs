using PaymentGateway.Domain.Model;
using PaymentGateway.Domain.Repositories;
using PaymentGateway.Domain.Validators;
using System.Threading.Tasks;

namespace PaymentGateway.Application.Specifications
{
    public class CurrencyExists : ICurrencyExistRule
    {
        private readonly ICurrencyRepository _currencyRepository;

        public static string messageFormat = "CurrencyNotExists";

        public CurrencyExists(ICurrencyRepository currencyRepository)
        {
            _currencyRepository = currencyRepository;
        }

        public async Task<(bool isValid, string error, Currency currency)> IsValidAsync(string name)
        {
            var currency = await _currencyRepository.GetByAsync(name);

            if (currency == null)
                return(false,string.Format(messageFormat, name), null);

            return (true, string.Empty, currency);
        }
    }
}
