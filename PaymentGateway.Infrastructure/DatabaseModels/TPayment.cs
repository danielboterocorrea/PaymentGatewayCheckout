using System;

namespace PaymentGateway.Infrastructure.DatabaseModels
{
    public class TPayment
    {
        public Guid Id { get; set; }
        public Guid MerchantId { get; set; }
        public string CreditCard { get; set; }
        public decimal Amount { get; set; }
        public Guid CurrencyId { get; set; }
        public string StatusCode { get; set; }
        public string Reason { get; set; }

        public virtual TMerchant Merchant { get; set; }
        public virtual TCurrency Currency { get; set; }
    }
}
