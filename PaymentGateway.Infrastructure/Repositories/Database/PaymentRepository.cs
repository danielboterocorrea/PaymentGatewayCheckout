using Microsoft.EntityFrameworkCore;
using PaymentGateway.Domain.Model;
using PaymentGateway.Domain.Repositories;
using PaymentGateway.Infrastructure.DatabaseModels;
using System;
using System.Threading.Tasks;
using System.Linq;
using PaymentGateway.Domain.Toolbox.Interfaces;
using PaymentGateway.Infrastructure.Repositories.Mappers;
using PaymentGateway.Domain.Common;
using System.Collections.Generic;

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
                MerchantId = payment.Merchant.Id,
                Amount = payment.Amount,
                CreditCard = payment.CreditCard.Encrypt(_cryptor),
                CurrencyId = (await GetCurrencyBy(payment.Currency.Value)).Id,
                StatusCode = payment.StatusCode.ToString()
            };

            await _unitOfWork.Payments.AddAsync(tpayment);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<TCurrency> GetCurrencyBy(string value)
        {
            return  await(from currency in _unitOfWork.Currencies
                                   where currency.Currency == value
                                   select currency).FirstOrDefaultAsync();
        }

        public async Task<Payment> GetAsync(Guid id)
        {
            var tpayment = await(from payment in _unitOfWork.Payments.AsNoTracking()
                                 .Include(p => p.Merchant)
                                 .Include(p => p.Currency)
                                 where payment.Id == id
                                 select payment).FirstOrDefaultAsync();

            if (tpayment == null)
                return null;

            return CreatePayment(tpayment);
        }

        public Payment CreatePayment(TPayment tpayment)
        {
            return new Payment(tpayment.Id, MerchantMapper.From(tpayment.Merchant),
                CreditCardMapper.From(_cryptor, tpayment.CreditCard),
                tpayment.Amount, CurrencyMapper.From(tpayment.Currency),
                (StatusCode)Enum.Parse(typeof(StatusCode), tpayment.StatusCode), tpayment.Reason);
        }

        public async Task UpdateAsync(Payment payment)
        {
            var tpayment = await(from paymentDb in _unitOfWork.Payments.AsNoTracking()
                                 .Include(p => p.Merchant)
                                 .Include(p => p.Currency)
                                 where paymentDb.Id == payment.Id
                                 select paymentDb).FirstOrDefaultAsync();

            var local = _unitOfWork.Set<TPayment>().Local.FirstOrDefault(p => p.Id == payment.Id);
            // check if local is not null 
            if (local != null)
            {
                // detach
                _unitOfWork.Entry(local).State = EntityState.Detached;
            }
            _unitOfWork.Entry(tpayment).State = EntityState.Modified;

            tpayment.StatusCode = payment.StatusCode.ToString();
            tpayment.Reason = payment.Reason;
            _unitOfWork.Payments.Update(tpayment);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IList<Payment>> GetAsync()
        {
            var tpayments = await (from paymentDb in _unitOfWork.Payments.AsNoTracking()
                                  .Include(p => p.Merchant)
                                  .Include(p => p.Currency)
                          select paymentDb).ToListAsync();

            return tpayments.Select(tp => CreatePayment(tp)).ToList();
        }
    }
}

