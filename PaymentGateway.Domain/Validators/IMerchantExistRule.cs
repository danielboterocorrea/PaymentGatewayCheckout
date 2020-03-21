using PaymentGateway.Domain.Model;
using System.Threading.Tasks;

namespace PaymentGateway.Domain.Validators
{
    public interface IMerchantExistRule
    {
        Task<(bool isValid, string error, Merchant merchant)> IsValidAsync(string name);
    }
}
