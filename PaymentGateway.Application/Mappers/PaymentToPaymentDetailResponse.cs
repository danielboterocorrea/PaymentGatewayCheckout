using PaymentGateway.Application.Mappers.Interfaces;
using PaymentGateway.Application.ResponseModels;
using PaymentGateway.Domain.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentGateway.Application.Mappers
{
    public class PaymentToPaymentDetailResponse : IPaymentToPaymentDetailResponse
    {
        public PaymentDetailResponse Map(Payment payment)
        {
            return new PaymentDetailResponse
            {
                Id = payment.Id,
                Amount = payment.Amount,
                CreditCard = new RequestModels.CreditCardInfo
                {
                    Number = CreditCard.MaskNumber(payment.CreditCard.Number),
                    ExpirationDate = payment.CreditCard.ExpirationDate,
                    Cvv = payment.CreditCard.Cvv,
                    HolderName = payment.CreditCard.HolderName
                },
                Currency = payment.Currency.Value,
                Merchant = new RequestModels.MerchantInfo
                {
                    Name = payment.Merchant.Name
                }
            };
        }
    }
}
