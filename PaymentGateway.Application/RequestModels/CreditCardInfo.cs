using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentGateway.Application.RequestModels
{
    public class CreditCardInfo
    {
        public string Number { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int Cvv { get; set; }
        public string HolderName { get; set; }
    }
}
