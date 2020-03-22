using PaymentGateway.Application.Services.Interfaces;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PaymentGateway.Infrastructure.Toolbox
{
    public class InMemoryQueue<T> : IPublisher<T>, IQueueProvider<T>
    {
        public BlockingCollection<T> Requests;

        public InMemoryQueue()
        {
            Requests = new BlockingCollection<T>(new ConcurrentQueue<T>());
        }

        public IEnumerable<T> GetConsumingEnumerable()
        {
            return Requests.GetConsumingEnumerable();
        }

        public void Publish(T request)
        {
            Requests.Add(request);
        }
    }
}
