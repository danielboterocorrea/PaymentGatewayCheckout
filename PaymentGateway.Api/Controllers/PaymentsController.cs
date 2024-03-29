﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PaymentGateway.Api.Helpers;
using PaymentGateway.Application.Mappers.Interfaces;
using PaymentGateway.Application.RequestModels;
using PaymentGateway.Application.Services.Interfaces;
using PaymentGateway.Domain.Exceptions;
using PaymentGateway.Domain.Metrics;

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
        private readonly IConfiguration _configuration;
        private static string ControllerName = $"api/{nameof(PaymentsController)}".Replace("Controller", string.Empty);
        private string Host => _configuration["PaymentGateway:Host"];

        public PaymentsController(IPaymentService paymentService,
            IPaymentToPaymentDetailResponse paymentToPaymentDetailResponse, 
            ILogger<PaymentsController> logger,
            IMetricsTime metricsTime,
            IMetricsCounter metricsCounter, IConfiguration configuration)
        {
            _paymentService = paymentService;
            _paymentToPaymentDetailResponse = paymentToPaymentDetailResponse;
            _logger = logger;
            _metricsTime = metricsTime;
            _metricsCounter = metricsCounter;
            _configuration = configuration;
        }

        

        // GET: api/Payments
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            var payments = await _paymentService.RetrieveAllAsync();
            return Ok(payments
                .Select(p =>  
                ApiReponseActionResult.CreateResponse(_paymentToPaymentDetailResponse.Map(p), ControllerName, Host)));
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

            if (payment == null)
                return NotFound(ApiReponseActionResult.CreateNotFoundResponse(id));

            return Ok(ApiReponseActionResult.CreateResponse(
                    _paymentToPaymentDetailResponse.Map(payment),
                    ControllerName, Host));
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
                    _logger.LogDebug($"Payment passed validations [Id: {paymentRequest}]");
                }
                _logger.LogDebug($"Start executing on process success");
                _paymentService.OnProcessSuccessAsync(paymentRequest);
                _logger.LogDebug($"Stop executing on process success");
            }
            catch(InvalidPaymentRequestException invalidPaymentRequestException)
            {
                _metricsCounter.IncrementCounter(MetricsCountsData.PaymentsReceivedErrors);
                _logger.LogError(invalidPaymentRequestException, $"Post - PaymentRequest id {paymentRequest.ToString()}");
                return BadRequest(ApiReponseActionResult.CreateInvalid(invalidPaymentRequestException.Message.Split(",")));
            }
            finally
            {
                sw.Stop();
                _metricsTime.RecordTime(MetricsTimeData.TimePaymentsReceived, sw.Elapsed);
            }

            return Ok(ApiReponseActionResult.CreateResponse(paymentRequest.Id, ControllerName, Host));
        }
    }
}
