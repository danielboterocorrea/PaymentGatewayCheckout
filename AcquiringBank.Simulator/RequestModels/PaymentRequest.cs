using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AcquiringBank.Simulator.RequestModels
{
    public class PaymentRequest
    {
        public Guid Id { get; set; }
        public MerchantInfo Merchant { get; set; }
        public CreditCardInfo CreditCard { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
    }
}
