using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentGateway.Domain.Exceptions
{
    public class InvalidPaymentRequestException : InvalidOperationException
    {
        public InvalidPaymentRequestException(string error) : base(error)
        {

        }
    }
}
