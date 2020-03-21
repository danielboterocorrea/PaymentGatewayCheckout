using PaymentGateway.Domain.Model;
using System.Threading.Tasks;

namespace PaymentGateway.Domain.Validators
{
    public interface ICurrencyExistRule
    {
        Task<(bool isValid, string error, Currency currency)> IsValidAsync(string name);
    }
}
