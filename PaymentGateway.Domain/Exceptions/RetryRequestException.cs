using System;

namespace PaymentGateway.Domain.Exceptions
{
    public class RetryRequestException : Exception
    {
        public RetryRequestException(string error) : base(error)
        {

        }
    }
}
