using NUnit.Framework;
using PaymentGateway.Application.Mapper;
using PaymentGateway.SharedTests;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentGateway.IntegrationTests.Application
{
    [TestFixture]
    public class PaymentServiceTests
    {
        [Test]
        public void PaymentRequestToPaymentValidDataTest()
        {
            var paymentRequest = TestsHelper.GetValidPaymentRequest();
            var paymentRequestToPayment = new PaymentRequestToPayment(TestsHelper.GetCreditCardRules(),
                TestsHelper.GetPaymentAmountRules(),
                TestsHelper.GetMerchantRule(),
                TestsHelper.GetCurrencyRule());

            var payment = paymentRequestToPayment.MapAsync(paymentRequest).GetAwaiter().GetResult();

        }

        [Test]
        public void ProcessPaymentInvalidCardTest()
        {

        }
    }
}
