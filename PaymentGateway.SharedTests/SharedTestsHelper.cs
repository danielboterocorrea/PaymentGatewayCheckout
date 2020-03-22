using AcquiringBank.Simulator.Common;
using AcquiringBank.Simulator.ResponseModels;
using IdentityModel.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json;
using PaymentGateway.Application.Mapper;
using PaymentGateway.Application.RequestModels;
using PaymentGateway.Application.Services;
using PaymentGateway.Application.Services.Interfaces;
using PaymentGateway.Application.Specifications;
using PaymentGateway.Domain.Model;
using PaymentGateway.Domain.Specifications;
using PaymentGateway.Domain.Toolbox;
using PaymentGateway.Domain.Toolbox.Interfaces;
using PaymentGateway.Domain.Validators;
using PaymentGateway.Infrastructure;
using PaymentGateway.Infrastructure.DatabaseModels;
using PaymentGateway.Infrastructure.Repositories;
using PaymentGateway.Infrastructure.Repositories.Cache;
using PaymentGateway.Infrastructure.Toolbox;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PaymentGateway.SharedTests
{
    public class SharedTestsHelper
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

        public static string GetValidPaymentRequestResponse(string id)
        {
            return "{\"result\":{\"id\":\"" + id + "\",\"merchant\":{\"name\":\"Apple\"},\"creditCard\":" +
                "{\"number\":\"XXXX XXXX XXXX 1213\",\"expirationDate\":\"2025-01-01T00:00:00\"," +
                "\"cvv\":0,\"holderName\":\"Daniel Botero Correa\"},\"amount\":125.0,\"currency\":\"EUR\"," +
                "\"statusCode\":\"Pending\",\"reason\":null},\"_links\":[{\"self\":{\"href\":\"Payments/" + id + "\"}}]}";
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

        public static PaymentGatewayContext GetPaymentGatewayContext()
        {
            var paymentGatewayContext = new PaymentGatewayContext(new DbContextOptionsBuilder<PaymentGatewayContext>()
                .UseInMemoryDatabase(databaseName: "PaymentGatewayInMemoryDatabaseTests")
                .Options);

            DatabaseSeeding(paymentGatewayContext);
            return paymentGatewayContext;
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

        public static InMemoryQueue<PaymentRequest> GetInMemoryQueue()
        {
            return new InMemoryQueue<PaymentRequest>();
        }

        public static ICryptor GetCryptor()
        {
            return new Cryptor(Configuration["PaymentGateway:CryptoSecret"]);
        }

        public static InMemoryGatewayCache GetCache()
        {
            var logger = new NullLogger<InMemoryGatewayCache>();
            return new InMemoryGatewayCache(logger);
        }

        public static PaymentRequestToPayment GetPaymentRequestToPayment()
        {
            return new PaymentRequestToPayment(GetCreditCardRules(),
                GetPaymentAmountRules(),
                GetMerchantRule(),
                GetCurrencyRule());
        }

        public static PaymentService GetPaymentService(PaymentGatewayContext paymentGatewayContext, IPublisher<PaymentRequest> producerConsumer)
        {
            var logger = new NullLogger<PaymentService>();
            var cryptor = GetCryptor();
            var paymentRepository = new PaymentRepository(cryptor, paymentGatewayContext);
            return new PaymentService(paymentRepository, GetPaymentRequestToPayment(), logger, producerConsumer);
        }

        public static HttpClient GetHttpClientAcquiringSimulatorMockedFailure(Guid paymentRequestId)
        {
            return GetHttpClient(new PaymentReponse
            {
                Id = Guid.NewGuid(),
                PaymentId = paymentRequestId,
                StatusCode = StatusCode.Failure.ToString()
            });
        }

        public static HttpClient GetHttpClientAcquiringSimulatorMockedSuccess(Guid paymentRequestId)
        {
            return GetHttpClient(new PaymentReponse
            {
                Id = Guid.NewGuid(),
                PaymentId = paymentRequestId,
                StatusCode = StatusCode.Success.ToString()
            });
        }

        public static HttpClient GetHttpClient(PaymentReponse response)
        {
            var body = JsonConvert.SerializeObject(response);
            var httpClient = new Mock<HttpClient>();
            var responseMessage = new HttpResponseMessage();
            responseMessage.StatusCode = System.Net.HttpStatusCode.OK;
            var httpContent = new StringContent(body, Encoding.UTF8, "application/json");
            responseMessage.Content = httpContent;
            httpClient.Setup(cli =>
                    cli.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(responseMessage));

            return httpClient.Object;
        }

        private static IConfiguration configuration = null;
        public static IConfiguration Configuration { get{
            if(configuration == null)
            {
                configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory))
                .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.Testing.json", optional: false, reloadOnChange: true)
                .Build();
            }
            return configuration;
        }
}

        public static ISendItem<T, R> GetSendPayment<T, R>(HttpClient httpClient) where T : IGetId
        {
            

            var cryptor = GetCryptor();
            var cache = GetCache();
            var context = GetPaymentGatewayContext();
            var paymentRepository = new PaymentCacheRepository(new PaymentRepository(cryptor, context),
                TestLogger.Create<PaymentCacheRepository>(), cache);
            var acquiringBankPaymentService = new AcquiringBankPaymentService(TestLogger.Create<AcquiringBankPaymentService>(),
                httpClient, paymentRepository, configuration);
            var timeOutHttpRequest = new TimeoutRequest<T, R>((ISendItem<T, R>)acquiringBankPaymentService,
                TestLogger.Create<TimeoutRequest<T, R>>(), TimeSpan.FromSeconds(20));
            var retries = new RetryRequest<T, R>(timeOutHttpRequest,
                TestLogger.Create<RetryRequest<T, R>>(), 3);

            return retries;
        }
        
        public static ConsumerSender<T, R> GetProducerConsumerSender<T, R>(HttpClient httpClient, IQueueProvider<T> producerConsumer) where T : IGetId
        {
            var retries = GetSendPayment<T, R>(httpClient);
            return new ConsumerSender<T, R>(TestLogger.Create<ConsumerSender<T, R>>(), 3, retries, (IQueueProvider<T>)producerConsumer);
        }

        public static PaymentService GetPaymentServiceWithMockedPublisher()
        {
            var logger = new NullLogger<PaymentService>();
            var queryProvider = new Mock<IPublisher<PaymentRequest>>();
            var cryptor = GetCryptor();
            var paymentRepository = new PaymentRepository(cryptor, GetPaymentGatewayContext());
            return new PaymentService(paymentRepository, GetPaymentRequestToPayment(), logger, queryProvider.Object);
        }

        public static bool PaymentExists(Guid id)
        {
            return (from tPayment in GetPaymentGatewayContext()
                           .Payments.AsNoTracking()
                            where tPayment.Id == id
                            select tPayment).Count() == 1;
        }

        public static bool PaymentExistsWithStatusCode(Guid id, Domain.Common.StatusCode statusCode)
        {
            return (from tPayment in GetPaymentGatewayContext()
                           .Payments.AsNoTracking()
                    where tPayment.Id == id
                    && tPayment.StatusCode == statusCode.ToString()
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

        public static void LaunchIdentityServer()
        {
            Task.Factory.StartNew(() =>
            {
                Host.CreateDefaultBuilder()
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                    .UseUrls("https://*:5002", "http://*:5003")
                    .UseStartup<IdentityServer.Startup>();
                }).Build().Run();
            });
        }

        public static void LaunchAcquiringSimulator()
        {
            Task.Factory.StartNew(() =>
            {
                Host.CreateDefaultBuilder()
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                    .UseUrls(new[] { "http://*:53677", "https://*:44398" })
                    .UseStartup<AcquiringBank.Simulator.Startup>();
                }).Build().Run();
            });
        }

        public static string GetAccessToken()
        {
            var identityServerFactory = new WebApplicationFactory<IdentityServer.Startup>();
            var identityClient = identityServerFactory.CreateClient();
            identityClient.BaseAddress = new Uri("https://localhost:5002");
            var disco = identityClient.GetDiscoveryDocumentAsync("https://localhost:5002").GetAwaiter().GetResult();
            if (disco.IsError)
            {
                throw new Exception("IdentityServer not found");
            }

            // request token
            var tokenResponse = identityClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "Apple",
                ClientSecret = "678ebc03-8fb1-407f-ac5e-ff97e8b810f5",
                Scope = "PaymentGatewayApi"
            }).GetAwaiter().GetResult();

            if (tokenResponse.IsError)
            {
                throw new Exception(tokenResponse.Error);
            }

            return tokenResponse.AccessToken;
        }
    }
}
