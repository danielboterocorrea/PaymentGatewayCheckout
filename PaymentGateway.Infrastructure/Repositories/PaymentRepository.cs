using Microsoft.EntityFrameworkCore;
using PaymentGateway.Domain.Model;
using PaymentGateway.Domain.Repositories;
using PaymentGateway.Infrastructure.DatabaseModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using PaymentGateway.Domain.Toolbox.Interfaces;
using PaymentGateway.Infrastructure.Repositories.Mappers;

namespace PaymentGateway.Infrastructure.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ICryptor _cryptor;
        private readonly PaymentGatewayContext _unitOfWork;

        public PaymentRepository(ICryptor cryptor,
            PaymentGatewayContext unitOfWork)
        {
            _cryptor = cryptor;
            _unitOfWork = unitOfWork;
        }
        public async Task AddAsync(Payment payment)
        {
            var tpayment = new TPayment
            {
                Id = payment.Id,
                Merchant = MerchantMapper.From(payment.Merchant),
                Amount = payment.Amount,
                CreditCard = payment.CreditCard.Encrypt(_cryptor),
                Currency = await GetCurrencyBy(payment.Currency.Value),
                StatusCode = payment.StatusCode.ToString()
            };

            await _unitOfWork.AddAsync(tpayment);
        }

        private async Task<TCurrency> GetCurrencyBy(string value)
        {
            return  await(from currency in _unitOfWork.Currencies
                                   where currency.Currency == value
                                   select currency).FirstOrDefaultAsync();
        }

        public async Task<Payment> GetAsync(Guid id)
        {
            var tpayment = await(from payment in _unitOfWork.Payments
                                 .Include(p => p.Merchant)
                                 .Include(p => p.Currency)
                                 where payment.Id == id
                                 select payment).FirstOrDefaultAsync();

            if (tpayment == null)
                return null;

            return new Payment(tpayment.Id, MerchantMapper.From(tpayment.Merchant),
                CreditCardMapper.From(_cryptor, tpayment.CreditCard), 
                tpayment.Amount, CurrencyMapper.From(tpayment.Currency), 
                (StatusCode)Enum.Parse(typeof(StatusCode), tpayment.StatusCode));
        }
    }
}

