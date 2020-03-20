using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Toolbox.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PaymentGateway.Application.Toolbox
{
    public class PaymentProducerConsumer : IProducerConsumer
    {
        private BlockingCollection<WorkItem> _taskQ;
        private readonly ILogger<PaymentProducerConsumer> _logger;

        public PaymentProducerConsumer(ILogger<PaymentProducerConsumer> logger, int workerCount)
        {
            _taskQ = new BlockingCollection<WorkItem>();
            _logger = logger;

            _logger.LogInformation($"Initializing threads [{workerCount}]");

            for (int i = 0; i < workerCount; i++)
                Task.Factory.StartNew(Consume);
        }

        public Task EnqueueTask(Action action, CancellationToken? cancelToken = null)
        {
            _logger.LogDebug($"EnqueueTask");
            var tcs = new TaskCompletionSource<object>();
            _taskQ.Add(new WorkItem(tcs, action, cancelToken));
            return tcs.Task;
        }

        public void Consume()
        {
            foreach (WorkItem workItem in _taskQ.GetConsumingEnumerable())
                if (workItem.CancelToken.HasValue &&
                    workItem.CancelToken.Value.IsCancellationRequested)
                {
                    workItem.TaskSource.SetCanceled();
                }
                else
                    try
                    {
                        _logger.LogDebug($"Consuming Task");
                        workItem.Action();
                        workItem.TaskSource.SetResult(null);   // Indicate completion
                    }
                    catch (OperationCanceledException ex)
                    {
                        if (ex.CancellationToken == workItem.CancelToken)
                            workItem.TaskSource.SetCanceled();
                        else
                            workItem.TaskSource.SetException(ex);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error treating a WorkItem");
                        workItem.TaskSource.SetException(ex);
                    }
        }

        public void Dispose()
        {
            _taskQ.CompleteAdding();
        }
    }
}
