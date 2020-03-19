using PaymentGateway.Domain.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PaymentGateway.Domain.Repositories
{
    public interface ICurrencyRepository
    {
        Task<Currency> GetByAsync(string value);
    }
}
