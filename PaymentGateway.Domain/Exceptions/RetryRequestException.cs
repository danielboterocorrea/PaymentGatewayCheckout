using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentGateway.Domain.Exceptions
{
    public class RetryRequestException : Exception
    {
        public RetryRequestException(string error) : base(error)
        {

        }
    }
}
