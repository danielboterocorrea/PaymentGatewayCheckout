using PaymentGateway.Domain.Model;
using System;
using System.Threading.Tasks;

namespace PaymentGateway.Domain.Repositories
{
    public interface IPaymentRepository
    {
        Task AddAsync(Payment payment);

        Task<Payment> GetAsync(Guid id);
    }
}
