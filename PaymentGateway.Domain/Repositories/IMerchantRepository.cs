using PaymentGateway.Domain.Model;
using System.Threading.Tasks;

namespace PaymentGateway.Domain.Repositories
{
    public interface IMerchantRepository
    {
        Task<Merchant> GetByAsync(string name);
    }
}
