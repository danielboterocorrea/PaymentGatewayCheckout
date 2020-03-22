using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Services.Interfaces;
using PaymentGateway.Domain.Exceptions;
using PaymentGateway.Domain.Toolbox;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PaymentGateway.Infrastructure.Toolbox
{
    public class RetryRequest<T, R> : ISendItem<T, R> where T : IGetId
    {
        private readonly ISendItem<T, R> _sendService;
        private readonly ILogger<RetryRequest<T, R>> _logger;
        private readonly int _retries;
        public RetryRequest(ISendItem<T, R> sendService,
            ILogger<RetryRequest<T, R>> logger, int retries)
        {
            _sendService = sendService;
            _logger = logger;
            _retries = retries;
        }

        public async Task<R> SendAsync(T item, CancellationToken externalCancellationToken)
        {
            int tryNumber = 0;

            _logger.LogInformation($"{nameof(RetryRequest<T, R>)} SendAsync for {typeof(T)} [{item.GetId()}]");

            while (true)
            {
                try
                {
                    return await _sendService.SendAsync(item, externalCancellationToken);
                }
                catch (Exception ex)
                {
                    if (tryNumber >= _retries)
                    {
                        _logger.LogError(ex, $"Something went wrong in RetryHttpRequest, check the {nameof(RetryRequestException)}");
                        throw new RetryRequestException($"Request retries [{_retries}] exceeded for {typeof(T)} [{item.GetId()}");
                    }
                    tryNumber++;
                    _logger.LogInformation($"Retry number[{tryNumber}] for {typeof(T)} [{item.GetId()}]");
                }
            }
        }
    }
}
