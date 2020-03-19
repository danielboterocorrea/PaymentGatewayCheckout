using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentGateway.Infrastructure.DatabaseModels
{
    public class TPayment
    {
        public Guid Id { get; set; }
        public TMerchant Merchant { get; set; }
        public string CreditCard { get; set; }
        public decimal Amount { get; set; }
        public TCurrency Currency { get; set; }
        public string StatusCode { get; set; }
    }
}
