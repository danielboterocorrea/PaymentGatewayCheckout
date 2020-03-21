using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PaymentGateway.Application.RequestModels;
using PaymentGateway.Application.ResponseModels;
using PaymentGateway.Application.Services.Interfaces;
using PaymentGateway.Application.Toolbox.Interfaces;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PaymentGateway.Application.Services
{
    public class AcquiringBankPaymentService : ISendItem
    {
        private readonly ILogger<AcquiringBankPaymentService> _logger;
        private readonly HttpClient _httpClient;

        public AcquiringBankPaymentService(ILogger<AcquiringBankPaymentService> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<AcquiringBankPaymentResponse> SendAsync(PaymentRequest paymentRequest, CancellationToken cancellationToken)
        {
            var body = JsonConvert.SerializeObject(paymentRequest);
            _logger.LogInformation($"Sending Payment request [{paymentRequest.Id}] to AcquiringBank");
            // Wrap our JSON inside a StringContent which then can be used by the HttpClient class
            var httpContent = new StringContent(body, Encoding.UTF8, "application/json");
            //TODO: hard coded url
            var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:44398/api/Payments")
            {
                Content = httpContent
            };
            var result = await _httpClient.SendAsync(request, cancellationToken);

            //Throws HttpRequestException when The HTTP response is unsuccessful.
            result.EnsureSuccessStatusCode();
            _logger.LogInformation($"Payment request [{paymentRequest.Id}] to AcquiringBank succeed");
            AcquiringBankPaymentResponse response  = null;
            if (result.IsSuccessStatusCode)
            {
                response = JsonConvert.DeserializeObject<AcquiringBankPaymentResponse>(await result.Content.ReadAsStringAsync());
                await UpdatePayment(response, paymentRequest);
            }
            return response;
        }

        private async Task UpdatePayment(AcquiringBankPaymentResponse response, PaymentRequest paymentRequest)
        {
            _logger.LogInformation($"Updating Payment [{paymentRequest.Id}] to AcquiringBank succeed");
            //TODO: UpdatePayment
        }
    }
}
