using PaymentGateway.Domain.Model;
using PaymentGateway.Infrastructure.DatabaseModels;

namespace PaymentGateway.Infrastructure.Repositories.Mappers
{
    public static class CurrencyMapper
    {
        public static Currency From(TCurrency currency)
        {
            return new Currency(currency.Currency);
        }
    }
}
