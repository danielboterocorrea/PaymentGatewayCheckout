using System;

namespace AcquiringBank.Simulator.ResponseModels
{
    public class PaymentReponse
    {
        public Guid Id;
        public Guid PaymentId { get; set; }
        public string StatusCode { get; set; }
        public string Reason { get; set; }
        public bool IsError => StatusCode != "Success";
    }
}
