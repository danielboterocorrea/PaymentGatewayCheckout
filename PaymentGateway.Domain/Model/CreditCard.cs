using PaymentGateway.Domain.Toolbox.Interfaces;
using PaymentGateway.Domain.Validators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PaymentGateway.Domain.Model
{
    public class CreditCard
    {
        public string Number { get; private set; }
        public DateTime ExpirationDate { get; private set; }
        public int Cvv { get; private set; }
        public string HolderName { get; private set; }

        private CreditCard(string number, DateTime expirationDate, int cvv, string holderName)
        {
            Number = number;
            ExpirationDate = expirationDate;
            Cvv = cvv;
            HolderName = holderName;
        }

        public static CreditCard NullObject = new CreditCard("", DateTime.MinValue, int.MinValue, "");

        public static CreditCard Create(string cardNumber, DateTime expirationDate, 
            int cvv, string holderName, 
            IList<ICreditCardRule> creditCardRules,
            out bool isValid, IList<string> errors)
        {
            isValid = true;

            cardNumber = cardNumber.Replace(" ", string.Empty).Trim();
            cardNumber = cardNumber.Replace("-", string.Empty).Trim();
            holderName = holderName.Trim();

            foreach (var rule in creditCardRules)
            {
                string error;
                if(!rule.IsValid(cardNumber, expirationDate, cvv,  holderName, out error))
                {
                    errors.Add(error);
                    isValid = false;
                }
            }

            if (errors.Any())
                return NullObject;

            return new CreditCard(cardNumber, expirationDate, cvv, holderName);
        }

        public string Encrypt(ICryptor cryptor)
        {
            return cryptor.Encrypt($"{Number}|{ExpirationDate}|{Cvv}|{HolderName}");
        }

        public static CreditCard Decrypt(ICryptor cryptor, string cryptedValue)
        {
            string creditCard = cryptor.Decrypt(cryptedValue);
            string[] creditCardValues = creditCard.Split('|');
            //TODO: Create using the create method, an extra validation may be good
            return new CreditCard(creditCardValues[0], 
                DateTime.Parse(creditCardValues[1]), 
                int.Parse(creditCardValues[2]), 
                creditCardValues[3]);
        }

        public static string MaskNumber(string number)
        {
            return $"XXXX XXXX XXXX " + number.Substring(12);
        }

        public override string ToString()
        {
            return $"CreditCard:[{MaskNumber(Number)} - {ExpirationDate} - {Cvv} - {HolderName}]";
        }
    }
}
