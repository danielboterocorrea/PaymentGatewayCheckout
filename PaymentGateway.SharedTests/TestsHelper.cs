using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PaymentGateway.Application.Mapper;
using PaymentGateway.Application.RequestModels;
using PaymentGateway.Application.Services;
using PaymentGateway.Application.Specifications;
using PaymentGateway.Domain.Model;
using PaymentGateway.Domain.Specifications;
using PaymentGateway.Domain.Toolbox;
using PaymentGateway.Domain.Toolbox.Interfaces;
using PaymentGateway.Domain.Validators;
using PaymentGateway.Infrastructure;
using PaymentGateway.Infrastructure.DatabaseModels;
using PaymentGateway.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PaymentGateway.SharedTests
{
    public class TestsHelper
    {
        public static CreditCard GetValidCreditCard()
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

            return creditCard;
        }

        public static CreditCard GetInvalidCreditCard()
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

            return creditCard;
        }

        public static PaymentRequest GetValidPaymentRequest()
        {
            return new PaymentRequest
            {
                Amount = 125,
                Currency = "EUR",
                CreditCard = new CreditCardInfo
                {
                    Cvv = 123,
                    ExpirationDate = new DateTime(2025, 01, 01),
                    HolderName = "Daniel Botero Correa",
                    Number = "1234 5678 9101 1213"
                },
                Id = Guid.NewGuid(),
                Merchant = new MerchantInfo
                {
                    Name = "Apple"
                }
            };
        }

        public static PaymentRequest GetInvalidPaymentRequest()
        {
            return new PaymentRequest
            {
                Amount = -5,
                Currency = "COP",
                CreditCard = new CreditCardInfo
                {
                    Cvv = 12,
                    ExpirationDate = new DateTime(2015, 01, 01),
                    HolderName = "",
                    Number = "1234_5678_9101_1213"
                },
                Id = Guid.NewGuid(),
                Merchant = new MerchantInfo
                {
                    Name = "Apples"
                }
            };
        }

        public static IList<ICreditCardRule> GetCreditCardRules()
        {
            var dateTimeProvider = new Mock<IDateTimeProvider>();
            dateTimeProvider.Setup(dp => dp.GetDateTimeNow()).Returns(new DateTime(2020, 01, 01));

            var validations = new List<ICreditCardRule>();
            validations.Add(new CardNumberNumeric16Digits());
            validations.Add(new Cvv3Numbers());
            validations.Add(new ExpiryDateHasntExpired(dateTimeProvider.Object));
            validations.Add(new HolderNotEmpty());
            return validations;
        }

        public static IList<IPaymentAmountRule> GetPaymentAmountRules()
        {
            var validations = new List<IPaymentAmountRule>();
            validations.Add(new NonNegativeAmount());
            return validations;
        }

        public static IMerchantExistRule GetMerchantRule()
        {
            var merchantRepository = new MerchantRepository(GetPaymentGatewayContext());
            return new MerchantExists(merchantRepository);
        }

        public static ICurrencyExistRule GetCurrencyRule()
        {
            var currencyRepository = new CurrencyRepository(GetPaymentGatewayContext());
            return new CurrencyExists(currencyRepository);
        }

        private static PaymentGatewayContext PaymentGatewayContext = null;
        public static PaymentGatewayContext GetPaymentGatewayContext()
        {
            if (PaymentGatewayContext == null)
            {
                PaymentGatewayContext = new PaymentGatewayContext(new DbContextOptionsBuilder<PaymentGatewayContext>()
                   .UseInMemoryDatabase(databaseName: "PaymentGatewayInMemoryDatabaseTests")
                   .Options);

                DatabaseSeeding(PaymentGatewayContext);
            }
            return PaymentGatewayContext;
        }

        private static void DatabaseSeeding(PaymentGatewayContext context)
        {
            // Look for any board games.
            if (!context.Merchants.Any())
            {
                context.Merchants.AddRange(
                    new TMerchant
                    {
                        Id = Guid.Parse("c24a92ef-25a2-446a-9505-c8844e915cf2"),
                        Name = "Apple"
                    },
                     new TMerchant
                     {
                         Id = Guid.Parse("2426dc82-ba6f-41d9-805a-18c831903a01"),
                         Name = "Google"
                     });
            }

            if (!context.Currencies.Any())
            {
                context.Currencies.AddRange(
                    new TCurrency
                    {
                        Id = Guid.Parse("369b0ac3-1f0c-4325-a6db-63ba5c8bfeb2"),
                        Currency = "EUR"
                    },
                     new TCurrency
                     {
                         Id = Guid.Parse("29af961b-780e-4d24-b512-67d0c0d6401d"),
                         Currency = "GBP"
                     },
                      new TCurrency
                      {
                          Id = Guid.Parse("130ba508-7f46-4833-8d08-664c87bd0c92"),
                          Currency = "USD"
                      });
            }

            context.SaveChanges();
        }

        public static ICryptor GetCryptor()
        {
            return new Cryptor("b8f73557-fa21-4d5a-9cfe-ec5e29c3d340");
        }

        public static PaymentRequestToPayment GetPaymentRequestToPayment()
        {
            return new PaymentRequestToPayment(GetCreditCardRules(),
                GetPaymentAmountRules(),
                GetMerchantRule(),
                GetCurrencyRule());
        }

        public static PaymentService GetPaymentService()
        {
            var logger = new NullLogger<PaymentService>();
            var cryptor = GetCryptor();
            var paymentRepository = new PaymentRepository(cryptor, GetPaymentGatewayContext());
            return new PaymentService(paymentRepository, GetPaymentRequestToPayment(), logger);
        }

        public static bool PaymentExists(Guid id)
        {
            return (from tPayment in GetPaymentGatewayContext()
                           .Payments.AsNoTracking()
                            where tPayment.Id == id
                            select tPayment).Count() == 1;
        }

        //https://blog.goyello.com/2011/11/23/entity-framework-invalid-operation/
        public static void DetachObjectFromDb(PaymentGatewayContext unitOfWork)
        {
            foreach (var dbEntityEntry in unitOfWork.ChangeTracker.Entries().ToArray())
            {
                if (dbEntityEntry.Entity != null)
                {
                    dbEntityEntry.State = EntityState.Detached;
                }
            }
        }

        public static void DeleteTables(PaymentGatewayContext unitOfWork)
        {
            unitOfWork.Payments.RemoveRange(unitOfWork.Payments);
            unitOfWork.SaveChanges();
            DetachObjectFromDb(unitOfWork);
        }
    }
}
