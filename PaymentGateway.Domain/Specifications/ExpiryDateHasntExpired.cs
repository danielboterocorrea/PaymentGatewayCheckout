using PaymentGateway.Domain.Toolbox;
using PaymentGateway.Domain.Validators;
using System;

namespace PaymentGateway.Domain.Specifications
{
    public class ExpiryDateHasntExpired : ICreditCardRule
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        public static string ExpiryDateHasExpired = "ExpiryDateHasExpired";

        public ExpiryDateHasntExpired(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }

        public bool IsValid(string cardNumber, DateTime expiryDate, int Cvv, string holderName, out string error)
        {
            error = null;

            if(expiryDate.Subtract(_dateTimeProvider.GetDateTimeNow()).TotalDays > 1)
            {
                return true;
            }

            error = ExpiryDateHasExpired;
            return false;
        }
    }
}
