using IdentityModel.Client;
using Newtonsoft.Json;
using PaymentGateway.Api.Helpers;
using PaymentGateway.Application.RequestModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Merchant.Simulator
{
    class Program
    {
        public static HttpClient httpClient = null;
        public static HttpClient GetClient()
        {
            if (httpClient == null)
                httpClient = new HttpClient();
            return httpClient;
        }


        public static string GetAccessToken()
        {
            var identityClient = GetClient();
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

        static async Task Main(string[] args)
        {
            Console.Title = "Merchant.Simulator";
            //SequenceRequests();
            await ParallelRequests();
        }
        private static double min = double.MaxValue;
        private static double max = double.MinValue;
        private static double sum = 0;
        private static object locker = new object();

        private async static Task ParallelRequests()
        {
            
            int loops = 1000;
            var tasks = new List<Task>();
            var paymentGatewayApiClient = GetClient();
            var accessToken = GetAccessToken();
            paymentGatewayApiClient.SetBearerToken(accessToken);

            for (int i = 0; i < loops; i++)
            {
                tasks.Add(Request(paymentGatewayApiClient, i));
                Thread.Sleep(20);
            }

            Task.WaitAll(tasks.ToArray());

            double avg = sum / loops;

            Console.WriteLine($"Max: {max} - Avg: {avg} - Min: {min}");
        }

        private async static Task Request(HttpClient paymentGatewayApiClient, int i)
        {
            var sw = new Stopwatch();

            var body = JsonConvert.SerializeObject(GetValidPaymentRequest());
            // Wrap our JSON inside a StringContent which then can be used by the HttpClient class
            var httpContent = new StringContent(body, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:44346/api/Payments")
            {
                Content = httpContent
            };
            CancellationTokenSource cts = new CancellationTokenSource();

            sw.Start();
            Console.WriteLine($"Task[{i}] - [0] - {sw.ElapsedMilliseconds}");
            var response = await paymentGatewayApiClient.SendAsync(request, cts.Token);
            Console.WriteLine($"Task[{i}] - [1] - {sw.ElapsedMilliseconds}");
            //Throws HttpRequestException when The HTTP response is unsuccessful.
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Task[{i}] - [2] - {sw.ElapsedMilliseconds}");
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(result);
            var id = ((dynamic)apiResponse.Result)["id"].Value;
            Console.WriteLine($"Task[{i}] - [3] - {sw.ElapsedMilliseconds}");
            var responseGet = await paymentGatewayApiClient.GetAsync($"https://localhost:44346/api/Payments/{id}");
            Console.WriteLine($"Task[{i}] - [4] - {sw.ElapsedMilliseconds}");
            responseGet.EnsureSuccessStatusCode();
            sw.Stop();
            lock (locker)
            {
                max = Math.Max(max, sw.Elapsed.TotalMilliseconds);
                min = Math.Min(min, sw.Elapsed.TotalMilliseconds);
                sum += sw.Elapsed.TotalMilliseconds;
            }
        }

        private static void SequenceRequests()
        {
            double min = double.MaxValue;
            double max = double.MinValue;
            double sum = 0;
            int loops = 10;

            var paymentGatewayApiClient = GetClient();
            var accessToken = GetAccessToken();
            paymentGatewayApiClient.SetBearerToken(accessToken);

            for (int i = 0; i < loops; i++)
            {
                var sw = new Stopwatch();

                var body = JsonConvert.SerializeObject(GetValidPaymentRequest());
                // Wrap our JSON inside a StringContent which then can be used by the HttpClient class
                var httpContent = new StringContent(body, Encoding.UTF8, "application/json");
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
                var responseGet = paymentGatewayApiClient.GetAsync($"https://localhost:44346/api/Payments/{id}").GetAwaiter().GetResult();
                responseGet.EnsureSuccessStatusCode();
                sw.Stop();
                max = Math.Max(max, sw.Elapsed.TotalMilliseconds);
                min = Math.Min(min, sw.Elapsed.TotalMilliseconds);
                sum += sw.Elapsed.TotalMilliseconds;
            }

            double avg = sum / loops;

            Console.WriteLine($"Max: {max} - Avg: {avg} - Min: {min}");
        }
    }
}
