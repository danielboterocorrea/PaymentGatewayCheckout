﻿using PaymentGateway.Application.RequestModels;
using System;

namespace PaymentGateway.Application.ResponseModels
{
    public class PaymentDetailResponse
    {
        public Guid Id { get; set; }
        public MerchantInfo Merchant { get; set; }
        public CreditCardInfo CreditCard { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string StatusCode { get; set; }
        public string Reason { get; set; }

    }
}
