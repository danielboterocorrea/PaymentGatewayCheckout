using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using PaymentGateway.Application.Services.Interfaces;
using PaymentGateway.Domain.Toolbox;
using PaymentGateway.Infrastructure.Toolbox;
using PaymentGateway.SharedTests;
using System;
using System.Net.Http;

namespace PaymentGateway.IntegrationTests.Helpers
{
    public class TestHelper
    {

        public static ConsumerSender<T, R> GetProducerConsumerSender<T, R>(HttpClient httpClient, WebApplicationFactory<FakePaymentGatewayApiStartup> paymentGatewayFactory, 
            IQueueProvider<T> producerConsumer) where T : IGetId
        {
            var retries = new FakeSendItemFactory<T, R>(paymentGatewayFactory.Services, httpClient);
            return new ConsumerSender<T, R>(TestLogger.Create<ConsumerSender<T, R>>(), 3, retries, producerConsumer);
        }

        public static HttpClient CreatePaymentGatewayHttpClient(WebApplicationFactory<FakePaymentGatewayApiStartup> paymentGatewayFactory)
        {
            var paymentGatewayApiClient = paymentGatewayFactory.CreateClient();
            paymentGatewayApiClient.SetBearerToken(SharedTestsHelper.GetAccessToken());
            paymentGatewayApiClient.BaseAddress = new Uri("https://localhost:44346");

            return paymentGatewayApiClient;
        }

        public static WebApplicationFactory<FakePaymentGatewayApiStartup> CreateCustomWebApplicationFactory()
        {
            return new CustomWebApplicationFactory<FakePaymentGatewayApiStartup>()
                            .WithWebHostBuilder(builder =>
                            {
                                builder.UseSolutionRelativeContentRoot("/");
                                builder.ConfigureTestServices(services =>
                                {
                                    services.AddMvc().AddApplicationPart(typeof(Api.Startup).Assembly);
                                });

                            });
        }
    }
}
