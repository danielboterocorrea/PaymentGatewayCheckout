using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PaymentGateway.Application.Toolbox.Interfaces
{
    public interface IProducerConsumer : IDisposable
    {
        Task EnqueueTask(Action action, CancellationToken? cancelToken = null);
        void Consume();
    }
}
