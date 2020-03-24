using PaymentGateway.Domain.Toolbox;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentGateway.Application.Services.Interfaces
{
    public interface ISentItemFactory<T,R> where T : IGetId
    {
        ISendItem<T, R> CreateSendItem();
    }
}
