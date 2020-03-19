using PaymentGateway.Domain.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PaymentGateway.Domain.Model
{
    public class CreditCard
    {
        public string Number { get; private set; }
        public DateTime ExpirationDate { get; private set; }
        public int Cvv { get; private set; }
        public string HolderName { get; private set; }

        private CreditCard(string number, DateTime expirationDate, int Cvv, string holderName)
        {
            Number = number;
            ExpirationDate = expirationDate;
            this.Cvv = Cvv;
            HolderName = holderName;
        }

        public static CreditCard NullObject = new CreditCard(null, DateTime.MinValue, int.MinValue, null);

        public static CreditCard Create(string cardNumber, DateTime expirationDate, 
            int cvv, string holderName, 
            IList<ICreditCardRule> creditCardRules,
            out bool isValid, IList<string> errors)
        {
            isValid = true;

            cardNumber = cardNumber.Replace(" ", string.Empty).Trim();
            holderName = holderName.Trim();

            foreach (var rule in creditCardRules)
            {
                string error;
                if(!rule.IsValid(cardNumber, expirationDate, cvv, out error))
                {
                    errors.Add(error);
                    isValid = false;
                }
            }

            if (errors.Any())
                return NullObject;

            return new CreditCard(cardNumber, expirationDate, cvv, holderName);
        }
    }
}
