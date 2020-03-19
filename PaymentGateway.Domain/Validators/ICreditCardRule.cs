using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentGateway.Domain.Validators
{
    public interface ICreditCardRule
    {
        bool IsValid(string cardNumber, DateTime expiryDate, int Cvv, string holderName, out string error);
    }
}
