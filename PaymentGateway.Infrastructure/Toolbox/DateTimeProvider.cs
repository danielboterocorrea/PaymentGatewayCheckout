using PaymentGateway.Domain.Toolbox;
using System;

namespace PaymentGateway.Infrastructure.Toolbox
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime GetDateTimeNow()
        {
            return DateTime.Now;
        }
    }
}
