using System;

namespace PaymentGateway.Domain.Toolbox
{
    public interface IDateTimeProvider
    {
        DateTime GetDateTimeNow();
    }
}
