using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        public PaymentsController(IPaymentService paymentService,
            IPaymentToPaymentDetailResponse paymentToPaymentDetailResponse)
        {
            _paymentService = paymentService;
            _paymentToPaymentDetailResponse = paymentToPaymentDetailResponse;
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
        public async Task Post([FromBody] PaymentRequest paymentRequest)
        {
            //TODO: ProducerConsumer implementation, can't be treated like this
            try
            {
                using (var transaction = new TransactionScope())
                {
                    var paymentId = await _paymentService.ProcessAsync(paymentRequest);
                    transaction.Complete();
                }
                await _paymentService.OnProcessSuccessAsync(paymentRequest);
            }
            catch(InvalidPaymentRequestException invalidPaymentRequestException)
            {
                //TODO: Invalid Response
            }
            
        }
    }
}
