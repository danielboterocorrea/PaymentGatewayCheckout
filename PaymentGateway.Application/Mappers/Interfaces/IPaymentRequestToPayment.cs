using PaymentGateway.Application.RequestModels;
using PaymentGateway.Domain.Model;
using System.Threading.Tasks;

namespace PaymentGateway.Application.Mappers.Interfaces
{
    public interface IPaymentRequestToPayment
    {
        Task<Payment> MapAsync(PaymentRequest payment);
    }
}
