using System;

namespace PaymentGateway.Domain.Exceptions
{
    public class InvalidPaymentRequestException : InvalidOperationException
    {
        public InvalidPaymentRequestException(string error) : base(error)
        {

        }
    }
}
