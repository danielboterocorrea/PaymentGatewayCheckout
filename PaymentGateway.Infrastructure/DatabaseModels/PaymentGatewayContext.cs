using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentGateway.Infrastructure.DatabaseModels
{
    public class PaymentGatewayContext : DbContext
    {
        public PaymentGatewayContext(DbContextOptions<PaymentGatewayContext> options)
            : base(options) { }

        public DbSet<TMerchant> Merchants { get; set; }
        public DbSet<TPayment> Payments { get; set; }
        public DbSet<TCurrency> Currencies { get; set; }
    }
}
