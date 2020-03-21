using Microsoft.Extensions.Logging;
using PaymentGateway.Application.RequestModels;
using PaymentGateway.Application.ResponseModels;
using PaymentGateway.Application.Services;
using PaymentGateway.Application.Toolbox.Interfaces;
using PaymentGateway.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PaymentGateway.Application.Toolbox
{
    public class TimeoutHttpRequest<T,R> : ISendItem<T,R> where T : IGetId
    {
        private readonly ISendItem<T,R> _sendService;
        private readonly ILogger<TimeoutHttpRequest<T, R>> _logger;
        private readonly TimeSpan _timeout;
        private readonly CancellationTokenSource _cancellationTokenSource;
        public TimeoutHttpRequest(ISendItem<T,R> sendService,
            ILogger<TimeoutHttpRequest<T, R>> logger, TimeSpan timeout)
        {
            _sendService = sendService;
            _logger = logger;
            _timeout = timeout;
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationTokenSource.CancelAfter(timeout);
        }

        public async Task<R> SendAsync(T item, CancellationToken externalCancellationToken)
        {
            _logger.LogInformation($"TimeoutHttpRequest SendAsync for {nameof(T)} [{item.Id}]");

            try
            {

            using (CancellationTokenSource linkedCts = CancellationTokenSource
                .CreateLinkedTokenSource(_cancellationTokenSource.Token,
                externalCancellationToken))
            {
                return await _sendService.SendAsync(item, linkedCts.Token);
            }

            }
            catch(OperationCanceledException operationCanceledException)
            {
                if(operationCanceledException.CancellationToken == _cancellationTokenSource.Token)
                {
                    var error = $"Timeout[{_timeout}] request operation for {nameof(T)} [{item.Id}]";
                    _logger.LogError(operationCanceledException, error);
                    throw new TimeOutRequestException(error);
                }
                throw;
            }
        }
    }
}
