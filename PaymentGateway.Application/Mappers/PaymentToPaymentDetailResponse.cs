using PaymentGateway.Application.Mappers.Interfaces;
using PaymentGateway.Application.ResponseModels;
using PaymentGateway.Domain.Model;

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
                    //We don't return the actual Cvv
                    Cvv = 000,
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
