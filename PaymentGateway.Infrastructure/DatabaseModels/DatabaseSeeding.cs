using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace PaymentGateway.Infrastructure.DatabaseModels
{
    public class DatabaseSeeding
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new PaymentGatewayContext(
                serviceProvider.GetRequiredService<DbContextOptions<PaymentGatewayContext>>()))
            {
                // Look for any board games.
                if (!context.Merchants.Any())
                {
                    context.Merchants.AddRange(
                        new TMerchant
                        {
                            Id = Guid.Parse("c24a92ef-25a2-446a-9505-c8844e915cf2"),
                            Name = "Apple"
                        },
                         new TMerchant
                         {
                             Id = Guid.Parse("2426dc82-ba6f-41d9-805a-18c831903a01"),
                             Name = "Google"
                         });
                }

                if(!context.Currencies.Any())
                {
                    context.Currencies.AddRange(
                        new TCurrency
                        {
                            Id = Guid.Parse("369b0ac3-1f0c-4325-a6db-63ba5c8bfeb2"),
                            Currency = "EUR"
                        },
                         new TCurrency
                         {
                             Id = Guid.Parse("29af961b-780e-4d24-b512-67d0c0d6401d"),
                             Currency = "GBP"
                         },
                          new TCurrency
                          {
                              Id = Guid.Parse("130ba508-7f46-4833-8d08-664c87bd0c92"),
                              Currency = "USD"
                          });
                }

                context.SaveChanges();
            }
        }
    }
}
