using PaymentGateway.Domain.Model;
using PaymentGateway.Domain.Toolbox.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

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
