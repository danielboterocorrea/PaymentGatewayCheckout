using PaymentGateway.Domain.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PaymentGateway.Domain.Validators
{
    public interface IMerchantExistRule
    {
        Task<(bool isValid, string error, Merchant merchant)> IsValidAsync(string name);
    }
}
