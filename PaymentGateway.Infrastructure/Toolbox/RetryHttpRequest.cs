using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Toolbox.Interfaces;
using PaymentGateway.Domain.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PaymentGateway.Infrastructure.Toolbox
{
    public class RetryHttpRequest<T, R> : ISendItem<T, R> where T : IGetId
    {
        private readonly ISendItem<T, R> _sendService;
        private readonly ILogger<RetryHttpRequest<T, R>> _logger;
        private readonly int _retries;
        public RetryHttpRequest(ISendItem<T, R> sendService,
            ILogger<RetryHttpRequest<T, R>> logger, int retries)
        {
            _sendService = sendService;
            _logger = logger;
            _retries = retries;
        }

        public async Task<R> SendAsync(T item, CancellationToken externalCancellationToken)
        {
            int tryNumber = 0;

            _logger.LogInformation($"TimeoutHttpRequest SendAsync for {typeof(T)} [{item.GetId()}]");

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
                        _logger.LogInformation(ex, $"Something went wrong in RetryHttpRequest, check the {nameof(RetryRequestException)}");
                        throw new RetryRequestException($"Request retries [{_retries}] exceeded for {typeof(T)} [{item.GetId()}");
                    }

                    tryNumber++;
                }
            }
        }
    }
}
