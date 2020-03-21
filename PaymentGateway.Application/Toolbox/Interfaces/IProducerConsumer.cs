using PaymentGateway.Application.RequestModels;
using System;
using System.Threading.Tasks;

namespace PaymentGateway.Application.Toolbox.Interfaces
{
    public interface IProducerConsumer<T> : IDisposable where T : IGetId
    {
        void EnqueuePayment(T request);
        Task Consume();
    }
}
