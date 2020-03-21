using Newtonsoft.Json;
using System;

namespace PaymentGateway.Application.ResponseModels
{
    public class AcquiringBankPaymentResponse
    {
        public Guid Id;
        public Guid PaymentId { get; set; }
        public string StatusCode { get; set; }
        public string Reason { get; set; }
        public bool IsError { get; set; }
        public string Error { get; set; }
        public string ErrorDescription { get; set; }
    }
}
