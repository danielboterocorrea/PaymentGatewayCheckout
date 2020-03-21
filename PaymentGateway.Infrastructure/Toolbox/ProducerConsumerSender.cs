using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Toolbox.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace PaymentGateway.Infrastructure.Toolbox
{
    public class ProducerConsumerSender<T, R> : IProducerConsumer<T> where T : IGetId
    {
        private BlockingCollection<T> _requests;
        private BlockingCollection<WorkItem<T, R>> _taskQ;
        private readonly ILogger<ProducerConsumerSender<T, R>> _logger;
        private readonly ISendItem<T, R> _sendPayment;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public ProducerConsumerSender(ILogger<ProducerConsumerSender<T, R>> logger,
            int workerCount, ISendItem<T, R> sendPayment)
        {
            logger.LogInformation($"Initializing threads [{workerCount}]");

            _taskQ = new BlockingCollection<WorkItem<T, R>>();
            _requests = new BlockingCollection<T>(new ConcurrentQueue<T>());
            _cancellationTokenSource = new CancellationTokenSource();
            _logger = logger;
            _sendPayment = sendPayment;

            for (int i = 0; i < workerCount; i++)
                Task.Factory.StartNew(Consume);

            Task.Factory.StartNew(EnqueueTask);
        }

        public void EnqueuePayment(T request)
        {
            _requests.Add(request);
        }

        private void EnqueueTask()
        {
            foreach (var request in _requests.GetConsumingEnumerable())
            {
                _logger.LogDebug($"EnqueueTask {typeof(T)} [{request.GetId()}]");
                EnqueueTask(_sendPayment.SendAsync(request, _cancellationTokenSource.Token), request);
            }
        }

        public void EnqueueTask(Task<R> task, T request)
        {
            _logger.LogDebug($"Enqueue {typeof(T)} [{request.GetId()}]");
            _taskQ.Add(new WorkItem<T, R>(task, request));
        }

        public async Task Consume()
        {
            foreach (var workItem in _taskQ.GetConsumingEnumerable())
            {
                try
                {
                    _logger.LogDebug($"Consuming {typeof(T)} [{workItem.item.GetId()}]");
                    var response = await workItem.Task;
                    _logger.LogInformation($"{workItem.item.GetId()} has been treated succesfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error treating {typeof(T)} [{workItem.item.GetId()}]");
                }

            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _taskQ.CompleteAdding();
            _requests.CompleteAdding();
        }
    }
}
