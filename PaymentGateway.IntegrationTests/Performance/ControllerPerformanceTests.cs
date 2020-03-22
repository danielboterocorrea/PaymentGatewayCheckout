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
    public static class TestLogger
    {
        public static ILogger<T> Create<T>()
        {
            var logger = new NUnitLogger<T>();
            return logger;
        }

        class NUnitLogger<T> : ILogger<T>, IDisposable
        {
            private readonly Action<string> output = Console.WriteLine;

            public void Dispose()
            {
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
                Func<TState, Exception, string> formatter) => output(formatter(state, exception));

            public bool IsEnabled(LogLevel logLevel) => true;

            public IDisposable BeginScope<TState>(TState state) => this;
        }
    }

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
        public void PaymentPostPerformanceTest(int loops)
        {
            double min = double.MaxValue;
            double max = double.MinValue;
            double sum = 0;

            var paymentGatewayApiClient = TestHelper.CreatePaymentGatewayHttpClient();
            //Warming up
            paymentGatewayApiClient.GetAsync($"/api/Payments/f7507410-6fe2-45c2-8f94-5a8ef8a66daa").GetAwaiter().GetResult();

            for (int i = 0; i < loops; i++)
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

            double avg = sum / loops;

            max.Should().BeLessThan(1000);
            min.Should().BeLessThan(80);
            avg.Should().BeLessThan(300);
            
            _logger.LogInformation($"Max: {max} - Avg: {avg} - Min: {min}");
        }

        [TestCase(10)]
        [TestCase(100)]
        [TestCase(1000)]
        public void PaymentPostMultithreadingPerformanceTest(int loops)
        {
            List<Task> tasks = new List<Task>();
            var paymentGatewayApiClient = TestHelper.CreatePaymentGatewayHttpClient();
            //Warming up
            paymentGatewayApiClient.GetAsync($"/api/Payments/f7507410-6fe2-45c2-8f94-5a8ef8a66daa").GetAwaiter().GetResult();

            for (int i = 0; i < loops; i++)
            {
                tasks.Add(RequestsAsync(paymentGatewayApiClient, i));
            }

            Task.WaitAll(tasks.ToArray());

            double avg = sum / loops;

            _logger.LogInformation($"#Tasks: {numberOfTasks} - Max: {max} - Avg: {avg} - Min: {min}");
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
