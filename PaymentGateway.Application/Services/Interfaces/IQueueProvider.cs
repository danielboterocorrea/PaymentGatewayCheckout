using System.Collections.Generic;

namespace PaymentGateway.Application.Services.Interfaces
{
    public interface IQueueProvider<T>
    {
        IEnumerable<T> GetConsumingEnumerable();
    }
}
