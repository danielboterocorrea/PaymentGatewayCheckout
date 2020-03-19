using PaymentGateway.Application.RequestModels;
using PaymentGateway.Domain.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PaymentGateway.Application.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<Guid> ProcessAsync(PaymentRequest paymentRequest);
        Task OnProcessSuccessAsync(PaymentRequest paymentRequest);
        Task<Payment> RetrieveAsync(Guid id);
    }
}
