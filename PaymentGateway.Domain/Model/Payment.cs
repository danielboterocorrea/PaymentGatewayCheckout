﻿using System;

namespace PaymentGateway.Domain.Model
{
    public class Payment
    {
        public Guid Id { get; }
        public Merchant Merchant { get; }
        public CreditCard CreditCard { get; }
        public decimal Amount { get; }
        public Currency Currency { get; }
        public StatusCode StatusCode { get; private set; }

        public string Reason { get; private set; }

        public Payment(Guid id, Merchant merchant, CreditCard creditCard, decimal amount, Currency currency, StatusCode statusCode)
        {
            Id = id;
            Merchant = merchant;
            CreditCard = creditCard;
            Amount = amount;
            Currency = currency;
            StatusCode = statusCode;
        }

        public void ChangeStatus(StatusCode statusCode, string reason)
        {
            StatusCode = StatusCode;
            Reason = reason;
        }

        public override string ToString()
        {
            return $"Payment:[{Id} - {Merchant} - {CreditCard} - {Amount} - {Currency} - {StatusCode}]";
        }
    }
}
