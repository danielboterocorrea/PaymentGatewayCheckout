using PaymentGateway.Application.Mappers.Interfaces;
using PaymentGateway.Application.RequestModels;
using PaymentGateway.Domain.Common;
using PaymentGateway.Domain.Exceptions;
using PaymentGateway.Domain.Model;
using PaymentGateway.Domain.Validators;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentGateway.Application.Mapper
{
    public class PaymentRequestToPayment : IPaymentRequestToPayment
    {
        private readonly IMerchantExistRule _merchantExistRule;
        private readonly ICurrencyExistRule _currencyExistRule;
        private readonly IList<ICreditCardRule> _creditCardRule;
        private readonly IList<IPaymentAmountRule> _paymentAmountRule;

        public PaymentRequestToPayment(IList<ICreditCardRule> creditCardRules,
            IList<IPaymentAmountRule> paymentAmountRules,
            IMerchantExistRule merchantRules,
            ICurrencyExistRule currencyExistRule)
        {
            _merchantExistRule = merchantRules;
            _currencyExistRule = currencyExistRule;
            _creditCardRule = creditCardRules;
            _paymentAmountRule = paymentAmountRules;
        }

        private bool AreValidatedPaymentRules(PaymentRequest payment, IList<string> errors)
        {
            foreach(var rule in _paymentAmountRule)
            {
                if(!rule.IsValid(payment.Amount, out string error))
                {
                    errors.Add(error);
                }
            }

            return !errors.Any();
        }

        private async Task<(bool isValid, string error, Merchant merchant)> AreValidatedMerchantRules(PaymentRequest payment)
        {
            var ruleApplied = await _merchantExistRule.IsValidAsync(payment.Merchant.Name);
            if (!ruleApplied.isValid)
            {
                return (false, ruleApplied.error, null);
            }
            return (true, string.Empty, ruleApplied.merchant);
        }

        private async Task<(bool isValid, string error, Currency currency)> AreValidatedCurrencyRules(PaymentRequest payment)
        {
            var ruleApplied = await _currencyExistRule.IsValidAsync(payment.Currency);
            if (!ruleApplied.isValid)
            {
                return (false, ruleApplied.error, null);
            }
            return (true, string.Empty, ruleApplied.currency);
        }

        public async Task<Payment> MapAsync(PaymentRequest payment)
        {
            var errors = new List<string>();
            
            bool isCreditCardValid;
            var creditCard = CreditCard.Create(payment.CreditCard.Number,
                payment.CreditCard.ExpirationDate,
                payment.CreditCard.Cvv,
                payment.CreditCard.HolderName,
                _creditCardRule,
                out isCreditCardValid,
                errors);

            AreValidatedPaymentRules(payment, errors);

            Merchant merchant = null;
            var merchantRule = await AreValidatedMerchantRules(payment);
            if(!merchantRule.isValid)
            {
                errors.Add(merchantRule.error);
            }
            else
            {
                merchant = merchantRule.merchant;
            }

            Currency currency = null;
            var currencyRule = await AreValidatedCurrencyRules(payment);
            if (!currencyRule.isValid)
            {
                errors.Add(currencyRule.error);
            }
            else
            {
                currency = currencyRule.currency;
            }

            if (errors.Any())
                throw new InvalidPaymentRequestException(string.Join(",", errors));

            return new Payment(payment.Id, merchant, creditCard, payment.Amount, currency, StatusCode.Pending);
        }
    }
}
