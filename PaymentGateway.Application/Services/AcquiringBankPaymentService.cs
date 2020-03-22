using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PaymentGateway.Application.RequestModels;
using PaymentGateway.Application.ResponseModels;
using PaymentGateway.Application.Services.Interfaces;
using PaymentGateway.Domain.Common;
using PaymentGateway.Domain.Repositories;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PaymentGateway.Application.Services
{
    public class AcquiringBankPaymentService : ISendItem<PaymentRequest, AcquiringBankPaymentResponse>
    {
        private readonly ILogger<AcquiringBankPaymentService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IConfiguration _configuration;

        public AcquiringBankPaymentService(ILogger<AcquiringBankPaymentService> logger, 
            HttpClient httpClient, IPaymentRepository paymentRepository, IConfiguration configuration)
        {
            _logger = logger;
            _httpClient = httpClient;
            _paymentRepository = paymentRepository;
            _configuration = configuration;
        }

        public async Task<AcquiringBankPaymentResponse> SendAsync(PaymentRequest paymentRequest, CancellationToken cancellationToken)
        {
            var body = JsonConvert.SerializeObject(paymentRequest);
            _logger.LogInformation($"Sending Payment request [{paymentRequest.Id}] to AcquiringBank");
            var httpContent = new StringContent(body, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, 
                $"{_configuration["AcquiringBank.EndPoints:Host"]}{_configuration["AcquiringBank.EndPoints:Payments"]}")
            {
                Content = httpContent
            };
            var result = await _httpClient.SendAsync(request, cancellationToken);

            result.EnsureSuccessStatusCode();
            _logger.LogInformation($"Payment request [{paymentRequest.Id}] to AcquiringBank succeed");
            AcquiringBankPaymentResponse response  = null;
            if (result.IsSuccessStatusCode)
            {
                response = JsonConvert.DeserializeObject<AcquiringBankPaymentResponse>(await result.Content.ReadAsStringAsync());
                await UpdatePaymentOnSuccess(response, paymentRequest);
            }
            return response;
        }

        private async Task UpdatePaymentOnSuccess(AcquiringBankPaymentResponse response, PaymentRequest paymentRequest)
        {
            _logger.LogInformation($"Updating Payment [{paymentRequest.Id}] to AcquiringBank succeed");
            var payment = await _paymentRepository.GetAsync(response.PaymentId);
            payment.ChangeStatus((StatusCode)Enum.Parse(typeof(StatusCode), response.StatusCode),
                response.Reason);
            await _paymentRepository.UpdateAsync(payment);
        }
    }
}
