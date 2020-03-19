using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentGateway.Domain.Toolbox
{
    public interface IDateTimeProvider
    {
        DateTime GetDateTimeNow();
    }
}
