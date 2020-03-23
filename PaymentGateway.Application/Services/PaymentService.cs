using System;
using PaymentGateway.Domain.Model;
using PaymentGateway.Application.RequestModels;
using PaymentGateway.Application.Services.Interfaces;
using PaymentGateway.Domain.Repositories;
using System.Threading.Tasks;
using PaymentGateway.Application.Mappers.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace PaymentGateway.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentRequestToPayment _paymentRequestToPayment;
        private readonly ILogger<PaymentService> _logger;
        private readonly IPublisher<PaymentRequest> _queueProvider;

        public PaymentService(IPaymentRepository paymentRepository,
            IPaymentRequestToPayment paymentRequestToPayment,
            ILogger<PaymentService> logger,
            IPublisher<PaymentRequest> producerConsumer)
        {
            _paymentRepository = paymentRepository;
            _paymentRequestToPayment = paymentRequestToPayment;
            _logger = logger;
            _queueProvider = producerConsumer;
        }

        public async Task<Guid> ProcessAsync(PaymentRequest paymentRequest)
        {
            _logger.LogInformation($"Processing payment: {paymentRequest.Id}");
            var payment = await _paymentRequestToPayment.MapAsync(paymentRequest);
            await _paymentRepository.AddAsync(payment);
            return payment.Id;
        }

        public void OnProcessSuccessAsync(PaymentRequest paymentRequest)
        {
            _queueProvider.Publish(paymentRequest);
            _logger.LogInformation($"OnProcessSuccessAsync queued: {paymentRequest.Id}");
        }

        public async Task<Payment> RetrieveAsync(Guid id)
        {
            return await _paymentRepository.GetAsync(id);
        }

        public async Task<IList<Payment>> RetrieveAllAsync()
        {
            return await _paymentRepository.GetAsync();
        }
    }
}
