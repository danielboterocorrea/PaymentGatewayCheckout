using PaymentGateway.Domain.Validators;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentGateway.Domain.Specifications
{
    public class Cvv3Numbers : ICreditCardRule
    {
        public static string CvvMustContain3Numbers = "CvvMustContain3Numbers";
        public bool IsValid(string cardNumber, DateTime expiryDate, int Cvv, string holderName, out string error)
        {
            error = null;
            if(Cvv >= 100 && Cvv <= 999)
            {
                return true;
            }
            error = CvvMustContain3Numbers;
            return false;
        }
    }
}
