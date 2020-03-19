using System;
using PaymentGateway.Domain.Model;
using PaymentGateway.Application.RequestModels;
using PaymentGateway.Application.Services.Interfaces;
using PaymentGateway.Domain.Repositories;
using System.Threading.Tasks;
using System.Collections.Generic;
using PaymentGateway.Domain.Validators;
using PaymentGateway.Application.Mappers.Interfaces;

namespace PaymentGateway.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentRequestToPayment _paymentRequestToPayment;

        public PaymentService(IPaymentRepository paymentRepository,
            IPaymentRequestToPayment paymentRequestToPayment)
        {
            _paymentRepository = paymentRepository;
            _paymentRequestToPayment = paymentRequestToPayment;
        }

        public async Task<Guid> ProcessAsync(PaymentRequest paymentRequest)
        {
            var payment = await _paymentRequestToPayment.MapAsync(paymentRequest);
            await _paymentRepository.AddAsync(payment);
            return payment.Id;
        }

        public async Task OnProcessSuccessAsync(PaymentRequest paymentRequest)
        {
            throw new NotImplementedException();
        }

        public async Task<Payment> RetrieveAsync(Guid id)
        {
            return await _paymentRepository.GetAsync(id);
        }
    }
}
