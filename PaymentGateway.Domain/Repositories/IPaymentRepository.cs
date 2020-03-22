using PaymentGateway.Domain.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaymentGateway.Domain.Repositories
{
    public interface IPaymentRepository
    {
        Task<IList<Payment>> GetAsync();
        Task AddAsync(Payment payment);
        Task<Payment> GetAsync(Guid id);
        Task UpdateAsync(Payment payment);
    }
}
