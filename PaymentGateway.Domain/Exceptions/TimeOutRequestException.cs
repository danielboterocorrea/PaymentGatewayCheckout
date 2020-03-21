using System;

namespace PaymentGateway.Domain.Exceptions
{
    public class TimeOutRequestException : Exception
    {
        public TimeOutRequestException(string error) : base(error)
        {

        }
    }
}
