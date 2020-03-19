using PaymentGateway.Application.ResponseModels;
using PaymentGateway.Domain.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentGateway.Application.Mappers.Interfaces
{
    public interface IPaymentToPaymentDetailResponse
    {
        PaymentDetailResponse Map(Payment payment);
    }
}
