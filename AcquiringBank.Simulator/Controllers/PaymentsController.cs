using System;
using System.Threading;
using System.Threading.Tasks;
using AcquiringBank.Simulator.RequestModels;
using AcquiringBank.Simulator.ResponseModels;
using Microsoft.AspNetCore.Mvc;

namespace AcquiringBank.Simulator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly Random _random;

        public PaymentsController(Random random)
        {
            _random = random;
        }
        // POST: api/Payments
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] PaymentRequest paymentRequest)
        {
            Thread.Sleep(int.MaxValue);
            int value = _random.Next(0, 7);

            switch(value)
            {
                case 0:
                    return BadRequest("Oops");
                case 1:
                    return NotFound("Oops");
                case 2:
                    return Ok(new PaymentReponse
                    {
                        Id = Guid.NewGuid(),
                        PaymentId = paymentRequest.Id,
                        StatusCode = "Success",
                    });
                case 3:
                    return Ok(new PaymentReponse
                    {
                        Id = Guid.NewGuid(),
                        PaymentId = paymentRequest.Id,
                        StatusCode = "Fail",
                        Reason = "Have no enough money"
                    });
                case 4:
                    return Ok(new PaymentReponse
                    {
                        Id = Guid.NewGuid(),
                        PaymentId = paymentRequest.Id,
                        StatusCode = "Fail",
                        Reason = "Looks like this is a fraudulent transaction"
                    });
                case 5:
                    return BadRequest("Woow");

                case 6:
                    Thread.Sleep(int.MaxValue);
                    break;

                default:
                    return Ok();
            }

            return Ok();
        }
    }
}
