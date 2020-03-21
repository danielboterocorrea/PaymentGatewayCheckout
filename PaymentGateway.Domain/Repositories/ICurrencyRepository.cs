using PaymentGateway.Domain.Model;
using System.Threading.Tasks;

namespace PaymentGateway.Domain.Repositories
{
    public interface ICurrencyRepository
    {
        Task<Currency> GetByAsync(string value);
    }
}
