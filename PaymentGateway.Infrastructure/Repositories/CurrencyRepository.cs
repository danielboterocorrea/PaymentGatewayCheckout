using Microsoft.EntityFrameworkCore;
using PaymentGateway.Domain.Model;
using PaymentGateway.Domain.Repositories;
using PaymentGateway.Infrastructure.DatabaseModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace PaymentGateway.Infrastructure.Repositories
{
    public class CurrencyRepository : ICurrencyRepository
    {
        private readonly PaymentGatewayContext _unitOfWork;

        public CurrencyRepository(PaymentGatewayContext unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Currency> GetByAsync(string value)
        {
            var currencyDb = await (from currency in _unitOfWork.Currencies
                   where currency.Currency == value
                   select currency).FirstOrDefaultAsync();

            if (currencyDb == null)
                return null;

            return new Currency(currencyDb.Currency);
        }
    }
}
