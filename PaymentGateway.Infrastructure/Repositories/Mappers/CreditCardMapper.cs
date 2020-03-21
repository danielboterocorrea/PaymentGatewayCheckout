using PaymentGateway.Domain.Model;
using PaymentGateway.Domain.Toolbox.Interfaces;

namespace PaymentGateway.Infrastructure.Repositories.Mappers
{
    public static class CreditCardMapper
    {
        public static CreditCard From(ICryptor cryptor, string encryptedValue)
        {
            return CreditCard.Decrypt(cryptor, encryptedValue);
        }
    }
}
