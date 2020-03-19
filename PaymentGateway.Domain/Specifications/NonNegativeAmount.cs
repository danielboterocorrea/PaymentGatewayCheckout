using PaymentGateway.Domain.Validators;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentGateway.Domain.Specifications
{
    public class NonNegativeAmount : IPaymentAmountRule
    {
        public static string NonNegativeAmountViolation = "NonNegativeAmountViolation";

        public bool IsValid(decimal amount, out string error)
        {
            error = null;
            if(amount < 0)
            {
                error = NonNegativeAmountViolation;
                return false;
            }

            return true;
        }
    }
}
