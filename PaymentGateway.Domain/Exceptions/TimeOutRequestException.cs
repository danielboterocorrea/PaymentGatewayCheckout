using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentGateway.Domain.Exceptions
{
    public class TimeOutRequestException : Exception
    {
        public TimeOutRequestException(string error) : base(error)
        {

        }
    }
}
