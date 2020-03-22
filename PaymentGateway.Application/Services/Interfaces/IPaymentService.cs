using PaymentGateway.Application.RequestModels;
using PaymentGateway.Domain.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaymentGateway.Application.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<Guid> ProcessAsync(PaymentRequest paymentRequest);
        void OnProcessSuccessAsync(PaymentRequest paymentRequest);
        Task<Payment> RetrieveAsync(Guid id);
        Task<IList<Payment>> RetrieveAllAsync();
    }
}
