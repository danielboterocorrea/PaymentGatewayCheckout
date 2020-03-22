using PaymentGateway.Domain.Toolbox;
using System.Threading;
using System.Threading.Tasks;

namespace PaymentGateway.Application.Services.Interfaces
{
    public interface ISendItem<T, R> where T : IGetId
    {
        Task<R> SendAsync(T item, CancellationToken cancellationToken);
    }
}
