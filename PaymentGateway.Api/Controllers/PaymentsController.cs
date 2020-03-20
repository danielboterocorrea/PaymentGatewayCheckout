using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PaymentGateway.Api.Helpers;
using PaymentGateway.Application.Mappers.Interfaces;
using PaymentGateway.Application.RequestModels;
using PaymentGateway.Application.ResponseModels;
using PaymentGateway.Application.Services.Interfaces;
using PaymentGateway.Application.Toolbox.Interfaces;
using PaymentGateway.Domain.Exceptions;
using PaymentGateway.Domain.Metrics;
using PaymentGateway.Domain.Repositories;
using PaymentGateway.Domain.Validators;
using Prometheus;

namespace PaymentGateway.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IPaymentToPaymentDetailResponse _paymentToPaymentDetailResponse;
        private readonly ILogger<PaymentsController> _logger;
        private readonly IMetricsTime _metricsTime;
        private readonly IMetricsCounter _metricsCounter;
        private static string ControllerName = $"{nameof(PaymentsController)}".Replace("Controller", string.Empty);

        public PaymentsController(IPaymentService paymentService,
            IPaymentToPaymentDetailResponse paymentToPaymentDetailResponse, 
            ILogger<PaymentsController> logger,
            IMetricsTime metricsTime,
            IMetricsCounter metricsCounter)
        {
            _paymentService = paymentService;
            _paymentToPaymentDetailResponse = paymentToPaymentDetailResponse;
            _logger = logger;
            _metricsTime = metricsTime;
            _metricsCounter = metricsCounter;
        }

        // GET: api/Payments/5
        [HttpGet("{id}", Name = "Get")]
        public async Task<ActionResult> Get(Guid id)
        {
            _metricsCounter.IncrementCounter(MetricsCountsData.PaymentsRetrieved);
            var sw = Stopwatch.StartNew();

            _logger.LogInformation($"Retrieving {id}");
            var payment = await _paymentService.RetrieveAsync(id);

            sw.Stop();
            _metricsTime.RecordTime(MetricsTimeData.TimePaymentsRetrieved, sw.Elapsed);
            return Ok(ApiReponseActionResult.CreateResponse(
                    _paymentToPaymentDetailResponse.Map(payment),
                    ControllerName));
        }

        // POST: api/Payments
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] PaymentRequest paymentRequest)
        {
            _metricsCounter.IncrementCounter(MetricsCountsData.PaymentsReceived);
            var sw = Stopwatch.StartNew();

            _logger.LogInformation($"Payment received {paymentRequest.ToString()}");
            try
            {
                using (var transaction = new TransactionScope())
                {
                    var paymentId = await _paymentService.ProcessAsync(paymentRequest);
                    transaction.Complete();
                    _logger.LogInformation($"Payment passed validations [Id: {paymentRequest}]");
                }
                _logger.LogInformation($"Start executing on process success");
                await _paymentService.OnProcessSuccessAsync(paymentRequest);
                _logger.LogInformation($"Stop executing on process success");
            }
            catch(InvalidPaymentRequestException invalidPaymentRequestException)
            {
                _metricsCounter.IncrementCounter(MetricsCountsData.PaymentsReceivedErrors);
                _logger.LogError(invalidPaymentRequestException, $"Post - PaymentRequest id {paymentRequest.ToString()}");
                return BadRequest(ApiReponseActionResult.CreateInvalid(invalidPaymentRequestException.Message.Split(","), 
                    ControllerName));
            }
            finally
            {
                sw.Stop();
                _metricsTime.RecordTime(MetricsTimeData.TimePaymentsReceived, sw.Elapsed);
            }

            return Ok(ApiReponseActionResult.CreateResponse(paymentRequest.Id, ControllerName));
        }
    }
}
