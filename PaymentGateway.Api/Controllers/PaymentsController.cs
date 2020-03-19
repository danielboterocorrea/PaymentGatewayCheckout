using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Mappers.Interfaces;
using PaymentGateway.Application.RequestModels;
using PaymentGateway.Application.ResponseModels;
using PaymentGateway.Application.Services.Interfaces;
using PaymentGateway.Domain.Exceptions;
using PaymentGateway.Domain.Repositories;
using PaymentGateway.Domain.Validators;

namespace PaymentGateway.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IPaymentToPaymentDetailResponse _paymentToPaymentDetailResponse;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IPaymentService paymentService,
            IPaymentToPaymentDetailResponse paymentToPaymentDetailResponse, ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _paymentToPaymentDetailResponse = paymentToPaymentDetailResponse;
            _logger = logger;
        }

        // GET: api/Payments/5
        [HttpGet("{id}", Name = "Get")]
        public async Task<PaymentDetailResponse> Get(Guid id)
        {
            var payment = await _paymentService.RetrieveAsync(id);
            return _paymentToPaymentDetailResponse.Map(payment);
        }

        // POST: api/Payments
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] PaymentRequest paymentRequest)
        {
            _logger.LogInformation($"Payment received {paymentRequest.ToString()}");
            //TODO: ProducerConsumer implementation, can't be treated like this
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
                _logger.LogError(invalidPaymentRequestException, $"Post - PaymentRequest id {paymentRequest.ToString()}");
                return BadRequest(string.Join(",", invalidPaymentRequestException.Message.Split(",")));
            }

            return Ok(paymentRequest.Id);
        }
    }
}
