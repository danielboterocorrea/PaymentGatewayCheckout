using PaymentGateway.Domain.Repositories;
using PaymentGateway.Domain.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PaymentGateway.Domain.Model
{
    public class Currency
    {
        public string Value { get; }

        public Currency(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"Currency:[{Value}]";
        }
    }
}
