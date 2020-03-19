using NUnit.Framework;
using PaymentGateway.Domain.Specifications;
using PaymentGateway.Domain.Toolbox;
using PaymentGateway.Domain.Validators;
using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using PaymentGateway.Domain.Model;
using FluentAssertions;
using PaymentGateway.Infrastructure;

namespace PaymentGateway.UnitTests.Domain
{
    [TestFixture]
    public class CreditCardTests
    {
        [Test]
        public void CreditCardValidationsAllValids()
        {
            var validations = new List<ICreditCardRule>();
            var dateTimeProvider = new Mock<IDateTimeProvider>();
                dateTimeProvider.Setup(dp => dp.GetDateTimeNow()).Returns(new DateTime(2020, 01, 01));


            validations.Add(new CardNumberNumeric16Digits());
            validations.Add(new Cvv3Numbers());
            validations.Add(new ExpiryDateHasntExpired(dateTimeProvider.Object));
            validations.Add(new HolderNotEmpty());

            var errors = new List<string>();

            var creditCard = CreditCard.Create("1234-5678-9101-1213", new DateTime(2025, 01, 01), 123, "Daniel Botero Correa", validations, out var isValid, errors);

            errors.Should().BeEmpty();
            isValid.Should().BeTrue();
            creditCard.Should().NotBeNull();
        }

        [Test]
        public void CreditCardValidationsAllInvalids()
        {
            var validations = new List<ICreditCardRule>();
            var dateTimeProvider = new Mock<IDateTimeProvider>();
            dateTimeProvider.Setup(dp => dp.GetDateTimeNow()).Returns(new DateTime(2020, 01, 01));


            validations.Add(new CardNumberNumeric16Digits());
            validations.Add(new Cvv3Numbers());
            validations.Add(new ExpiryDateHasntExpired(dateTimeProvider.Object));
            validations.Add(new HolderNotEmpty());

            var errors = new List<string>();

            var creditCard = CreditCard.Create("1234_5678_9101_1213", new DateTime(2019, 01, 01), 12, "", validations, out var isValid, errors);

            errors.Should().NotBeEmpty();
            errors.Should().Equal(new[] { 
                CardNumberNumeric16Digits.CardNumberMustBeNumeric16Digits,
                Cvv3Numbers.CvvMustContain3Numbers,
                ExpiryDateHasntExpired.ExpiryDateHasExpired,
                HolderNotEmpty.HolderNotEmptyViolation});

            isValid.Should().BeFalse();
            creditCard.Should().BeSameAs(CreditCard.NullObject);
        }

        [Test]
        public void CreditCardEncryptDecrypt()
        {
            var validations = new List<ICreditCardRule>();
            var dateTimeProvider = new Mock<IDateTimeProvider>();
            dateTimeProvider.Setup(dp => dp.GetDateTimeNow()).Returns(new DateTime(2020, 01, 01));
            var cryptor = new Cryptor("b8f73557-fa21-4d5a-9cfe-ec5e29c3d340");

            validations.Add(new CardNumberNumeric16Digits());
            validations.Add(new Cvv3Numbers());
            validations.Add(new ExpiryDateHasntExpired(dateTimeProvider.Object));
            validations.Add(new HolderNotEmpty());

            var errors = new List<string>();

            var creditCard = CreditCard.Create("1234 5678 9101 1213", new DateTime(2025, 01, 01), 123, "Daniel Botero Correa", validations, out var isValid, errors);

            errors.Should().BeEmpty();
            isValid.Should().BeTrue();
            creditCard.Should().NotBeNull();

            var encryptedCard = creditCard.Encrypt(cryptor);
            var decryptedCard = CreditCard.Decrypt(cryptor, encryptedCard);

            decryptedCard.Should().BeEquivalentTo(new
            {
                Number = creditCard.Number,
                ExpirationDate = creditCard.ExpirationDate,
                Cvv = creditCard.Cvv,
                HolderName = creditCard.HolderName
            });
        }
    }
}
