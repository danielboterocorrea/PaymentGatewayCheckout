using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Services;
using PaymentGateway.Application.Services.Interfaces;
using PaymentGateway.Domain.Toolbox;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace PaymentGateway.Infrastructure.Toolbox
{
    public class SendItemFactory<T, R> : ISentItemFactory<T, R> where T : IGetId
    {
        private readonly IServiceProvider _serviceProvider;

        public SendItemFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ISendItem<T, R> CreateSendItem()
        {
            var loggerFactory = _serviceProvider.GetService<ILoggerFactory>();
            var acquiringBankPaymentService = _serviceProvider.GetService<AcquiringBankPaymentService>();
            var timeOutHttpRequest = new TimeoutRequest<T, R>((ISendItem<T, R>)acquiringBankPaymentService,
                loggerFactory.CreateLogger<TimeoutRequest<T, R>>(), TimeSpan.FromSeconds(20));
            var retries = new RetryRequest<T, R>(timeOutHttpRequest,
                loggerFactory.CreateLogger<RetryRequest<T, R>>(), 3);

            return retries;
        }
    }
}
