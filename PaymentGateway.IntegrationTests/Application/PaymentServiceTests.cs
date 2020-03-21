using FluentAssertions;
using NUnit.Framework;
using PaymentGateway.Application.Mapper;
using PaymentGateway.Application.Specifications;
using PaymentGateway.Domain.Exceptions;
using PaymentGateway.Domain.Specifications;
using PaymentGateway.SharedTests;
using System;


namespace PaymentGateway.IntegrationTests.Application
{
    [TestFixture]
    public class PaymentServiceTests
    {
        [SetUp]
        public void SetUp()
        {
            TestsHelper.DeleteTables(TestsHelper.GetPaymentGatewayContext());
        }

        [Test]
        public void PaymentRequestToPaymentValidDataTest()
        {
            var paymentRequest = TestsHelper.GetValidPaymentRequest();
            var paymentRequestToPayment = new PaymentRequestToPayment(TestsHelper.GetCreditCardRules(),
                TestsHelper.GetPaymentAmountRules(),
                TestsHelper.GetMerchantRule(),
                TestsHelper.GetCurrencyRule());

            var payment = paymentRequestToPayment.MapAsync(paymentRequest).GetAwaiter().GetResult();

            payment.Should().BeEquivalentTo(new {
                Amount = paymentRequest.Amount,
                Currency = new
                {
                    Value = paymentRequest.Currency
                },
                CreditCard = new
                {
                    Cvv = 123,
                    ExpirationDate = new DateTime(2025, 01, 01),
                    HolderName = "Daniel Botero Correa",
                    Number = "1234567891011213"
                },
                Id = paymentRequest.Id,
                Merchant = new
                {
                    Name = "Apple"
                }});
        }

        [Test]
        public void PaymentRequestToPaymentInvalidDataTest()
        {
            var paymentRequest = TestsHelper.GetInvalidPaymentRequest();
            var paymentRequestToPayment = new PaymentRequestToPayment(TestsHelper.GetCreditCardRules(),
                TestsHelper.GetPaymentAmountRules(),
                TestsHelper.GetMerchantRule(),
                TestsHelper.GetCurrencyRule());

            Assert.Throws<InvalidPaymentRequestException>(() =>
            {
                paymentRequestToPayment.MapAsync(paymentRequest).GetAwaiter().GetResult();
            }, $"Should throw {nameof(InvalidPaymentRequestException)}");
        }

        [Test]
        public void PaymentRequestToPaymentInvalidDataMessagesTest()
        {
            var paymentRequest = TestsHelper.GetInvalidPaymentRequest();
            var paymentRequestToPayment = TestsHelper.GetPaymentRequestToPayment();

            try
            {
                paymentRequestToPayment.MapAsync(paymentRequest).GetAwaiter().GetResult();
            }
            catch(InvalidPaymentRequestException invalidPaymentRequestException)
            {
                invalidPaymentRequestException.Message.Split(',')
                    .Should()
                    .BeEquivalentTo(new[] {CardNumberNumeric16Digits.CardNumberMustBeNumeric16Digits,
                        Cvv3Numbers.CvvMustContain3Numbers,
                        HolderNotEmpty.HolderNotEmptyViolation,
                        NonNegativeAmount.NonNegativeAmountViolation,
                        ExpiryDateHasntExpired.ExpiryDateHasExpired,
                        string.Format(CurrencyExists.messageFormat, paymentRequest.Currency),
                        string.Format(MerchantExists.messageFormat, paymentRequest.Merchant.Name)},
                    o => o.WithoutStrictOrdering());
            }
        }

        [Test]
        public void ProcessPaymentInvalidCardMessageTest()
        {
            Assert.Throws<InvalidPaymentRequestException>(() =>
            {
                var paymentService = TestsHelper.GetPaymentService();
                paymentService.ProcessAsync(TestsHelper.GetInvalidPaymentRequest()).GetAwaiter().GetResult();
            }, $"Should throw {nameof(InvalidPaymentRequestException)}");
        }

        [Test]
        public void ProcessPaymentValidCardMessageTest()
        {

            var paymentService = TestsHelper.GetPaymentService();
            var paymentRequest = TestsHelper.GetValidPaymentRequest();
            var Guid = paymentService.ProcessAsync(paymentRequest).GetAwaiter().GetResult();

            Guid.Should().Be(paymentRequest.Id);
            
            TestsHelper.DetachObjectFromDb(TestsHelper.GetPaymentGatewayContext());

            TestsHelper.PaymentExists(Guid).Should().BeTrue();
        }
    }
}
