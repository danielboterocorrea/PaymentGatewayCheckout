﻿using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Services.Interfaces;
using PaymentGateway.Domain.Toolbox;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace PaymentGateway.Infrastructure.Toolbox
{
    public class ConsumerSender<T, R> : IConsumer<T> where T : IGetId
    {
        private BlockingCollection<WorkItem<T, R>> _taskQ;
        private readonly ILogger<ConsumerSender<T, R>> _logger;
        private readonly int _workerCount;
        private readonly ISentItemFactory<T, R> _sendPaymentFactory;
        private readonly IQueueProvider<T> _inMemoryQueue;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public ConsumerSender(ILogger<ConsumerSender<T, R>> logger,
            int workerCount, ISentItemFactory<T, R> sendPaymentFactory, IQueueProvider<T> inMemoryQueue)
        {
            logger.LogInformation($"Initializing threads [{workerCount}]");
            _taskQ = new BlockingCollection<WorkItem<T, R>>();
            _inMemoryQueue = inMemoryQueue;
            _cancellationTokenSource = new CancellationTokenSource();
            _logger = logger;
            _workerCount = workerCount;
            _sendPaymentFactory = sendPaymentFactory;
            
            for(int i = 0; i < _workerCount; i++)
                Task.Factory.StartNew(EnqueueTask);
        }

        private void EnqueueTask()
        {
            foreach (var request in _inMemoryQueue.GetConsumingEnumerable())
            {
                _logger.LogDebug($"EnqueueTask {typeof(T)} [{request.GetId()}]");
                EnqueueTask(_sendPaymentFactory.CreateSendItem().SendAsync(request, _cancellationTokenSource.Token), request);
            }
        }

        private void EnqueueTask(Task<R> task, T request)
        {
            _logger.LogDebug($"Enqueue {typeof(T)} [{request.GetId()}]");
            _taskQ.Add(new WorkItem<T, R>(task, request));
        }

        public async Task ConsumeAsync()
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
                    _logger.LogError(ex, $"Enqueuing {typeof(T)} [{workItem.item.GetId()}] after failure");
                    EnqueueTask(_sendPaymentFactory.CreateSendItem().SendAsync(workItem.item, _cancellationTokenSource.Token), workItem.item);
                }

            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _taskQ.CompleteAdding();
        }
    }
}
