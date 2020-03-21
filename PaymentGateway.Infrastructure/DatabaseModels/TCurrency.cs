using System;

namespace PaymentGateway.Infrastructure.DatabaseModels
{
    public class TCurrency
    {
        public Guid Id { get; set; }
        public string Currency { get; set; }
    }
}
