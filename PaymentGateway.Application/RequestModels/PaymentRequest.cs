﻿using PaymentGateway.Domain.Toolbox;
using System;

namespace PaymentGateway.Application.RequestModels
{
    public class PaymentRequest : IGetId
    {
        public Guid Id { get; set; }
        public MerchantInfo Merchant { get; set; }
        public CreditCardInfo CreditCard { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }

        public PaymentRequest()
        {
            Id = Guid.NewGuid();
        }

        public Guid GetId()
        {
            return Id;
        }
    }
}
