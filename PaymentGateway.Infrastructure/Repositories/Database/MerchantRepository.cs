using Microsoft.EntityFrameworkCore;
using PaymentGateway.Domain.Model;
using PaymentGateway.Domain.Repositories;
using PaymentGateway.Infrastructure.DatabaseModels;
using System.Threading.Tasks;
using System.Linq;

namespace PaymentGateway.Infrastructure.Repositories
{
    public class MerchantRepository : IMerchantRepository
    {
        private readonly PaymentGatewayContext _unitOfWork;

        public MerchantRepository(PaymentGatewayContext unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Merchant> GetByAsync(string value)
        {
            var merchantDb = await (from merchant in _unitOfWork.Merchants.AsNoTracking()
                                    where merchant.Name == value
                                    select merchant).FirstOrDefaultAsync();

            if (merchantDb == null)
                return null;

            return new Merchant(merchantDb.Id, merchantDb.Name);
        }
    }
}
