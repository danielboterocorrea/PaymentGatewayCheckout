using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Toolbox.Interfaces;
using PaymentGateway.Domain.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PaymentGateway.Infrastructure.Toolbox
{
    public class TimeoutRequest<T, R> : ISendItem<T, R> where T : IGetId
    {
        private readonly ISendItem<T, R> _sendService;
        private readonly ILogger<TimeoutRequest<T, R>> _logger;
        private readonly TimeSpan _timeout;
        public TimeoutRequest(ISendItem<T, R> sendService,
            ILogger<TimeoutRequest<T, R>> logger, TimeSpan timeout)
        {
            _sendService = sendService;
            _logger = logger;
            _timeout = timeout;
        }

        public async Task<R> SendAsync(T item, CancellationToken externalCancellationToken)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(_timeout);

            _logger.LogInformation($"{nameof(TimeoutRequest<T, R>)} SendAsync for {typeof(T)} [{item.GetId()}]");

            try
            {

                using (CancellationTokenSource linkedCts = CancellationTokenSource
                    .CreateLinkedTokenSource(cancellationTokenSource.Token,
                    externalCancellationToken))
                {
                    return await _sendService.SendAsync(item, linkedCts.Token);
                }

            }
            catch (OperationCanceledException operationCanceledException)
            {
                if (cancellationTokenSource.Token.IsCancellationRequested)
                {
                    var error = $"Timeout[{_timeout}] request operation for {typeof(T)} [{item.GetId()}]";
                    _logger.LogError(operationCanceledException, error);
                    throw new TimeOutRequestException(error);
                }
                throw;
            }
        }
    }
}
