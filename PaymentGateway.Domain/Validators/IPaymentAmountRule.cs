using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentGateway.Domain.Validators
{
    public interface IPaymentAmountRule
    {
        bool IsValid(decimal amount, out string error);
    }
}
