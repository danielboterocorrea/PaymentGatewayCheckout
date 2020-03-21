using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using PaymentGateway.SharedTests;
using System.Net;
using IdentityModel.Client;
using System;
using Microsoft.Extensions.Hosting;
using Serilog;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading;
using PaymentGateway.Application.ResponseModels;
using PaymentGateway.Api.Helpers;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore;
using System.Threading.Tasks;
using PaymentGateway.IntegrationTests.Helpers;

namespace PaymentGateway.IntegrationTests.Controllers
{
    [TestFixture]
    public class PaymentsControllerTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            SharedTestsHelper.LaunchIdentityServer();
        }

        [Test]
        public void PaymentNotFound()
        {
            var paymentGatewayApiClient = TestHelper.CreatePaymentGatewayHttpClient();
            var response = paymentGatewayApiClient.GetAsync("/api/Payments/620d725e-6cdb-4be5-a978-25eb38de1a53").GetAwaiter().GetResult();
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            response.Content
                .ReadAsStringAsync()
                .GetAwaiter()
                .GetResult()
                .Should()
                .Be("{\"error_type\":\"Failure\",\"error_codes\":[\"NotFound\"]}");
        }

        [Test]
        public void PostPaymentRequest()
        {
            var paymentGatewayApiClient = TestHelper.CreatePaymentGatewayHttpClient();

            var body = JsonConvert.SerializeObject(SharedTestsHelper.GetValidPaymentRequest());
            // Wrap our JSON inside a StringContent which then can be used by the HttpClient class
            var httpContent = new StringContent(body, Encoding.UTF8, "application/json");
            //TODO: hard coded url
            var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:44346/api/Payments")
            {
                Content = httpContent
            };
            CancellationTokenSource cts = new CancellationTokenSource();
            var response = paymentGatewayApiClient.SendAsync(request, cts.Token).GetAwaiter().GetResult();

            //Throws HttpRequestException when The HTTP response is unsuccessful.
            response.EnsureSuccessStatusCode();
            var result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(result);

            var id = ((dynamic)apiResponse.Result)["id"].Value;
            var responseGet = paymentGatewayApiClient.GetAsync($"/api/Payments/{id}").GetAwaiter().GetResult();
            responseGet.StatusCode.Should().Be(HttpStatusCode.OK);
            responseGet.Content.ReadAsStringAsync().GetAwaiter().GetResult().Should()
                .Be("{\"result\":{\"id\":\"" + id + "\",\"merchant\":{\"name\":\"Apple\"}," +
                "\"creditCard\":{\"number\":\"XXXX XXXX XXXX 1213\",\"expirationDate\":\"2025-01-01T00:00:00\",\"cvv\":0," +
                "\"holderName\":\"Daniel Botero Correa\"}," +
                "\"amount\":125.0,\"currency\":\"EUR\"}," +
                "\"_links\":[{\"self\":{\"href\":\"Payments/" + id + "\"}}]}");
        }
    }
}
