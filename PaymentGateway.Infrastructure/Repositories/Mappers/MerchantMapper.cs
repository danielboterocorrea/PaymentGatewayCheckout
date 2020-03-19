using PaymentGateway.Domain.Model;
using PaymentGateway.Infrastructure.DatabaseModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentGateway.Infrastructure.Repositories.Mappers
{
    public static class MerchantMapper
    {
        public static TMerchant From(Merchant merchant)
        {
            return new TMerchant
            {
                Id = merchant.Id,
                Name = merchant.Name
            };
        }

        public static Merchant From(TMerchant merchant)
        {
            return new Merchant(merchant.Id, merchant.Name);
        }
    }
}
