using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentGateway.Domain.Model
{
    public class Payment
    {
        public Guid Id { get; }
        public Merchant Merchant { get; }
        public CreditCard CreditCard { get; }
        public decimal Amount { get; }
        public Currency Currency { get; }
        public StatusCode StatusCode { get; }

        public Payment(Guid id, Merchant merchant, CreditCard creditCard, decimal amount, Currency currency, StatusCode statusCode)
        {
            Id = id;
            Merchant = merchant;
            CreditCard = creditCard;
            Amount = amount;
            Currency = currency;
            StatusCode = statusCode;
        }
    }
}
