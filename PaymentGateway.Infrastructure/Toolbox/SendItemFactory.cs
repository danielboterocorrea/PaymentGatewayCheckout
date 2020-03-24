using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Services;
using PaymentGateway.Application.Services.Interfaces;
using PaymentGateway.Domain.Repositories;
using PaymentGateway.Domain.Toolbox;
using System;
using System.Net.Http;

namespace PaymentGateway.Infrastructure.Toolbox
{
    public class SendItemFactory<T, R> : ISentItemFactory<T, R> where T : IGetId
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public SendItemFactory(ILoggerFactory loggerFactory, IPaymentRepository paymentRepository, 
            IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _loggerFactory = loggerFactory;
            _paymentRepository = paymentRepository;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public ISendItem<T, R> CreateSendItem()
        {
            var acquiringBankPaymentService = new AcquiringBankPaymentService(_loggerFactory.CreateLogger<AcquiringBankPaymentService>(),
                _httpClientFactory.CreateClient(), _paymentRepository, _configuration);
            var timeOutHttpRequest = new TimeoutRequest<T, R>((ISendItem<T, R>)acquiringBankPaymentService,
                _loggerFactory.CreateLogger<TimeoutRequest<T, R>>(), TimeSpan.FromSeconds(20));
            var retries = new RetryRequest<T, R>(timeOutHttpRequest,
                _loggerFactory.CreateLogger<RetryRequest<T, R>>(), 3);

            return retries;
        }
    }
}
