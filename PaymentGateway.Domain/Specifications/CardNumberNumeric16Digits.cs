using PaymentGateway.Domain.Validators;
using System;

namespace PaymentGateway.Domain.Specifications
{
    public class CardNumberNumeric16Digits : ICreditCardRule
    {
        public static string CardNumberMustBeNumeric16Digits = "CardNumberMustBeNumeric16Digits";
        public bool IsValid(string cardNumber, DateTime expiryDate, int Cvv, string holderName, out string error)
        {
            error = null;
            if(!AreAllNumbers(cardNumber))
            {
                error = CardNumberMustBeNumeric16Digits;
                return false;
            }

            return true;
        }

        private bool AreAllNumbers(string cardNumber)
        {
            foreach (char c in cardNumber)
            {
                int intC = (int)c;
                if (!(intC >= 48 && intC <= 57))
                    return false;
            }
            return true;
        }
    }
}
