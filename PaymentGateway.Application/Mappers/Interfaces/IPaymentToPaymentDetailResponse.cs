using PaymentGateway.Application.ResponseModels;
using PaymentGateway.Domain.Model;

namespace PaymentGateway.Application.Mappers.Interfaces
{
    public interface IPaymentToPaymentDetailResponse
    {
        PaymentDetailResponse Map(Payment payment);
    }
}
