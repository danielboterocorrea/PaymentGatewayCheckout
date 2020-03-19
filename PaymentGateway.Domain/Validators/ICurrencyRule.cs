using PaymentGateway.Domain.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PaymentGateway.Domain.Validators
{
    public interface ICurrencyExistRule
    {
        Task<(bool isValid, string error, Currency currency)> IsValidAsync(string name);
    }
}
