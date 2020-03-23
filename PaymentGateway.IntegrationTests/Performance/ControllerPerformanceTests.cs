using FluentAssertions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NUnit.Framework;
using PaymentGateway.Api.Helpers;
using PaymentGateway.IntegrationTests.Helpers;
using PaymentGateway.SharedTests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PaymentGateway.IntegrationTests.Performance
{


    [TestFixture]
    public class ControllerPerformanceTests
    {
        ILogger<ControllerPerformanceTests> _logger;
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            SharedTestsHelper.LaunchIdentityServer();
            SharedTestsHelper.LaunchAcquiringSimulator();
            _logger = TestLogger.Create<ControllerPerformanceTests>();
        }

        private double min;
        private double max;
        private double sum;
        private int numberOfTasks;
        private static object locker = new object();

        [SetUp]
        public void SetUp()
        {
            min = double.MaxValue;
            max = double.MinValue;
            sum = 0;
            numberOfTasks = 0;
        }

        [TestCase(2)]
        [TestCase(10)]
        [TestCase(100)]
        [TestCase(1000)]
        [TestCase(10000)]
        public void PaymentPostPerformanceTest(int requests)
        {
            double min = double.MaxValue;
            double max = double.MinValue;
            double sum = 0;

            var paymentGatewayApiClient = TestHelper.CreatePaymentGatewayHttpClient();
            //Warming up
            paymentGatewayApiClient.GetAsync($"/api/Payments/f7507410-6fe2-45c2-8f94-5a8ef8a66daa").GetAwaiter().GetResult();

            for (int i = 0; i < requests; i++)
            {
                var sw = new Stopwatch();

                var body = JsonConvert.SerializeObject(SharedTestsHelper.GetValidPaymentRequest());
                // Wrap our JSON inside a StringContent which then can be used by the HttpClient class
                var httpContent = new StringContent(body, Encoding.UTF8, "application/json");
                //TODO: hard coded url
                var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:44346/api/Payments")
                {
                    Content = httpContent
                };
                CancellationTokenSource cts = new CancellationTokenSource();

                sw.Start();
                var response = paymentGatewayApiClient.SendAsync(request, cts.Token).GetAwaiter().GetResult();
                //Throws HttpRequestException when The HTTP response is unsuccessful.
                response.EnsureSuccessStatusCode();
                var result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(result);
                var id = ((dynamic)apiResponse.Result)["id"].Value;
                var responseGet = paymentGatewayApiClient.GetAsync($"/api/Payments/{id}").GetAwaiter().GetResult();
                responseGet.StatusCode.Should().Be(HttpStatusCode.OK);
                responseGet.Content.ReadAsStringAsync().GetAwaiter().GetResult().Should()
                    .Be(SharedTestsHelper.GetValidPaymentRequestResponse(id));
                sw.Stop();
                max = Math.Max(max, sw.Elapsed.TotalMilliseconds);
                min = Math.Min(min, sw.Elapsed.TotalMilliseconds);
                sum += sw.Elapsed.TotalMilliseconds;
            }

            double avg = sum / requests;

            max.Should().BeLessThan(1000);
            min.Should().BeLessThan(80);
            avg.Should().BeLessThan(300);
            
            _logger.LogInformation($"Number of requests: {requests} - Time Max: {max} - Time Avg: {avg} - Time Min: {min}");
        }

        [TestCase(10)]
        [TestCase(100)]
        [TestCase(1000)]
        [TestCase(10000)]
        public void PaymentPostMultithreadingPerformanceTest(int parallelRequests)
        {
            List<Task> tasks = new List<Task>();
            var paymentGatewayApiClient = TestHelper.CreatePaymentGatewayHttpClient();
            //Warming up
            paymentGatewayApiClient.GetAsync($"/api/Payments/f7507410-6fe2-45c2-8f94-5a8ef8a66daa").GetAwaiter().GetResult();

            for (int i = 0; i < parallelRequests; i++)
            {
                tasks.Add(RequestsAsync(paymentGatewayApiClient, i));
            }

            Task.WaitAll(tasks.ToArray());

            double avg = sum / parallelRequests;

            _logger.LogInformation($"Parallel Requests: {parallelRequests} - Time Max: {max} - Time Avg: {avg} - Time Min: {min}");
        }

        private async Task RequestsAsync(HttpClient paymentGatewayApiClient, int i)
        {
            var sw = new Stopwatch();

            var body = JsonConvert.SerializeObject(SharedTestsHelper.GetValidPaymentRequest());
            // Wrap our JSON inside a StringContent which then can be used by the HttpClient class
            var httpContent = new StringContent(body, Encoding.UTF8, "application/json");
            //TODO: hard coded url
            var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:44346/api/Payments")
            {
                Content = httpContent
            };
            CancellationTokenSource cts = new CancellationTokenSource();

            sw.Start();
            var response = paymentGatewayApiClient.SendAsync(request, cts.Token).GetAwaiter().GetResult();
            Debug.WriteLine($"[{i}:1]:{sw.ElapsedMilliseconds}");
            //Throws HttpRequestException when The HTTP response is unsuccessful.
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(result);
            Debug.WriteLine($"[{i}:2]:{sw.ElapsedMilliseconds}");
            var id = ((dynamic)apiResponse.Result)["id"].Value;
            var responseGet = await paymentGatewayApiClient.GetAsync($"/api/Payments/{id}");
            Debug.WriteLine($"[{i}:3]:{sw.ElapsedMilliseconds}");
            responseGet.StatusCode.Should().Be(HttpStatusCode.OK);
            (await responseGet.Content.ReadAsStringAsync()).Should()
                .Be(SharedTestsHelper.GetValidPaymentRequestResponse(id));
            Debug.WriteLine($"[{i}:4]:{sw.ElapsedMilliseconds}");
            sw.Stop();
            lock (locker)
            {
                max = Math.Max(max, sw.Elapsed.TotalMilliseconds);
                min = Math.Min(min, sw.Elapsed.TotalMilliseconds);
                sum += sw.Elapsed.TotalMilliseconds;
                numberOfTasks++;
            }
        }
    }
}
