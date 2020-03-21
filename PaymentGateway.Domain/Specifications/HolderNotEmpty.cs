using PaymentGateway.Domain.Validators;
using System;

namespace PaymentGateway.Domain.Specifications
{
    public class HolderNotEmpty : ICreditCardRule
    {
        public static string HolderNotEmptyViolation = "HolderNotEmpty";
        public bool IsValid(string cardNumber, DateTime expiryDate, int Cvv, string holderName, out string error)
        {
            error = null;
            if (string.IsNullOrEmpty(holderName))
            {
                error = HolderNotEmptyViolation;
                return false;
            }
            return true;
        }
    }
}
