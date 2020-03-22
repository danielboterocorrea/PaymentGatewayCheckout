using System;
using System.Threading.Tasks;

namespace PaymentGateway.Application.Services.Interfaces
{
    public interface IConsumer<T> : IDisposable
    {
        Task ConsumeAsync();
    }
}
