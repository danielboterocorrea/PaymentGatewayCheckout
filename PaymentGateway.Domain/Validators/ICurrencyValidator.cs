using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentGateway.Domain.Validators
{
    public interface ICurrencyValidator
    {
        bool IsValid(string currency, out string error);
    }
}
