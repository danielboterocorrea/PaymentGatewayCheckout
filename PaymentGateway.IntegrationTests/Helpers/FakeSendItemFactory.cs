using PaymentGateway.Application.Services;
using PaymentGateway.Application.Services.Interfaces;
using PaymentGateway.Domain.Toolbox;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaymentGateway.Infrastructure.Toolbox;
using PaymentGateway.Domain.Repositories;
using PaymentGateway.SharedTests;
using System.Net.Http;

namespace PaymentGateway.IntegrationTests.Helpers
{
    public class FakeSendItemFactory<T, R> : ISentItemFactory<T, R> where T : IGetId
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HttpClient _httpClient;

        public FakeSendItemFactory(IServiceProvider serviceProvider, HttpClient httpClient)
        {
            _serviceProvider = serviceProvider;
            _httpClient = httpClient;
        }

        public ISendItem<T, R> CreateSendItem()
        {
            var paymentRepository=  _serviceProvider.GetService<IPaymentRepository>();
            var acquiringBankPaymentService = new AcquiringBankPaymentService(TestLogger.Create<AcquiringBankPaymentService>(),
                _httpClient, paymentRepository, SharedTestsHelper.Configuration);
            var timeOutHttpRequest = new TimeoutRequest<T, R>((ISendItem<T, R>)acquiringBankPaymentService,
                TestLogger.Create<TimeoutRequest<T, R>>(), TimeSpan.FromSeconds(20));
            var retries = new RetryRequest<T, R>(timeOutHttpRequest,
                TestLogger.Create<RetryRequest<T, R>>(), 3);

            return retries;
        }
    }
}
