using PaymentGateway.Application.Mappers.Interfaces;
using PaymentGateway.Application.RequestModels;
using PaymentGateway.Domain.Exceptions;
using PaymentGateway.Domain.Model;
using PaymentGateway.Domain.Repositories;
using PaymentGateway.Domain.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentGateway.Application.Mapper
{
    public class PaymentRequestToPayment : IPaymentRequestToPayment
    {
        private readonly IMerchantRepository _merchantRepository;
        private readonly ICurrencyRepository _currencyRepository;
        private readonly List<ICreditCardRule> _creditCardRule;

        public PaymentRequestToPayment(ICurrencyRepository currencyRepository,
            List<ICreditCardRule> creditCardRule,
            IMerchantRepository merchantRepository)
        {
            _currencyRepository = currencyRepository;
            _merchantRepository = merchantRepository;
            _creditCardRule = creditCardRule;
        }

        public async Task<Payment> MapAsync(PaymentRequest payment)
        {
            var errors = new List<string>();
            var currency = await _currencyRepository.GetAsync(payment.Currency);

            if (currency == null)
                errors.Add($"Currency {payment.Currency} was not found");

            bool isCreditCardValid;
            var creditCard = CreditCard.Create(payment.CreditCard.Number,
                payment.CreditCard.ExpirationDate,
                payment.CreditCard.Cvv,
                payment.CreditCard.HolderName,
                _creditCardRule,
                out isCreditCardValid,
                errors);

            if (!isCreditCardValid)
                errors.AddRange(errors);

            var merchant = await _merchantRepository.GetByAsync(payment.Merchant.Name);

            if (merchant == null)
                errors.Add($"Merchant {payment.Merchant.Name} was not found");

            if (errors.Any())
                throw new InvalidPaymentRequestException(string.Join(",", errors));

            return new Payment(payment.Id, merchant, creditCard, payment.Amount, currency, StatusCode.Pending);
        }
    }
}
