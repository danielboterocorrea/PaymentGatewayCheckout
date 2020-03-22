using IdentityModel.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using PaymentGateway.SharedTests;
using System;
using System.Net.Http;

namespace PaymentGateway.IntegrationTests.Helpers
{
    public class TestHelper
    {
        public static HttpClient CreatePaymentGatewayHttpClient()
        {
            var paymentGatewayFactory = new CustomWebApplicationFactory<FakePaymentGatewayApiStartup>()
                                        .WithWebHostBuilder(builder =>
                                        {
                                            builder.UseSolutionRelativeContentRoot("/");
                                            builder.ConfigureTestServices(services =>
                                            {
                                                services.AddMvc().AddApplicationPart(typeof(Api.Startup).Assembly);
                                            });

                                        });

            var paymentGatewayApiClient = paymentGatewayFactory.CreateClient();
            paymentGatewayApiClient.SetBearerToken(SharedTestsHelper.GetAccessToken());
            paymentGatewayApiClient.BaseAddress = new Uri("https://localhost:44346");

            return paymentGatewayApiClient;
        }
    }
}
