using PaymentGateway.Domain.Toolbox;
using System;
using System.Collections.Generic;
using System.Text;

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
