using System.Threading;
using System.Threading.Tasks;

namespace PaymentGateway.Application.Toolbox.Interfaces
{
    public interface ISendItem<T,R> where T : IGetId
    {
        Task<R> SendAsync(T item, CancellationToken cancellationToken);
    }
}
