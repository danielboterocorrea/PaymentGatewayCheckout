using System;
using PaymentGateway.Domain.Model;
using PaymentGateway.Application.RequestModels;
using PaymentGateway.Application.Services.Interfaces;
using PaymentGateway.Domain.Repositories;
using System.Threading.Tasks;
using System.Collections.Generic;
using PaymentGateway.Domain.Validators;
using PaymentGateway.Application.Mappers.Interfaces;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Toolbox.Interfaces;

namespace PaymentGateway.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentRequestToPayment _paymentRequestToPayment;
        private readonly ILogger<PaymentService> _logger;
        private readonly IProducerConsumer _producerConsumer;

        public PaymentService(IPaymentRepository paymentRepository,
            IPaymentRequestToPayment paymentRequestToPayment,
            ILogger<PaymentService> logger,
            IProducerConsumer producerConsumer)
        {
            _paymentRepository = paymentRepository;
            _paymentRequestToPayment = paymentRequestToPayment;
            _logger = logger;
            _producerConsumer = producerConsumer;
        }

        public async Task<Guid> ProcessAsync(PaymentRequest paymentRequest)
        {
            _logger.LogInformation($"Processing payment: {paymentRequest.Id}");
            var payment = await _paymentRequestToPayment.MapAsync(paymentRequest);
            await _paymentRepository.AddAsync(payment);
            return payment.Id;
        }

        public Task OnProcessSuccessAsync(PaymentRequest paymentRequest)
        {
            return _producerConsumer.EnqueueTask(() =>
            {
                _logger.LogInformation($"Sending payment {paymentRequest.Id} to Acquiring Bank");
            });
        }

        public async Task<Payment> RetrieveAsync(Guid id)
        {
            return await _paymentRepository.GetAsync(id);
        }
    }
}
