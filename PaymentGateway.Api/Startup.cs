using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Mapper;
using PaymentGateway.Application.Mappers;
using PaymentGateway.Application.Mappers.Interfaces;
using PaymentGateway.Application.Services;
using PaymentGateway.Application.Services.Interfaces;
using PaymentGateway.Application.Specifications;
using PaymentGateway.Domain.Repositories;
using PaymentGateway.Domain.Specifications;
using PaymentGateway.Domain.Toolbox;
using PaymentGateway.Domain.Toolbox.Interfaces;
using PaymentGateway.Domain.Validators;
using PaymentGateway.Infrastructure;
using PaymentGateway.Infrastructure.DatabaseModels;
using PaymentGateway.Infrastructure.Repositories;
using PaymentGateway.Infrastructure.Repositories.Cache;
using PaymentGateway.Infrastructure.Toolbox;

namespace PaymentGateway.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddDistributedMemoryCache();
            services.AddHttpContextAccessor();
            services.AddMemoryCache();

            services.AddDbContext<PaymentGatewayContext>(optionsBuilder => 
                   optionsBuilder.UseInMemoryDatabase(databaseName: "PaymentGatewayInMemoryDatabase"));

            //Repositories
            services.AddTransient<CurrencyRepository>();
            services.AddTransient<MerchantRepository>();
            services.AddTransient<PaymentRepository>();
            services.AddTransient<ICurrencyRepository,CurrencyCacheRepository>();
            services.AddTransient<IMerchantRepository, MerchantCacheRepository>();
            services.AddTransient<IPaymentRepository, PaymentRepository>();
            services.AddTransient<IPaymentToPaymentDetailResponse, PaymentToPaymentDetailResponse>();
            services.AddTransient<IPaymentRequestToPayment, PaymentRequestToPayment>(ps =>
            {
                var dtProvider = (IDateTimeProvider)ps.GetService(typeof(IDateTimeProvider));
                var merchantRepository = (IMerchantRepository)ps.GetService(typeof(IMerchantRepository));
                var currencyRepository = (ICurrencyRepository)ps.GetService(typeof(ICurrencyRepository));
                var creditCardRule = new List<ICreditCardRule>();
                creditCardRule.Add(new CardNumberNumeric16Digits());
                creditCardRule.Add(new Cvv3Numbers());
                creditCardRule.Add(new ExpiryDateHasntExpired(dtProvider));
                creditCardRule.Add(new HolderNotEmpty());

                var paymentRules = new List<IPaymentAmountRule>();
                paymentRules.Add(new NonNegativeAmount());

                return new PaymentRequestToPayment(creditCardRule,
                    paymentRules,
                    new MerchantExists(merchantRepository),
                    new CurrencyExists(currencyRepository));
            });

            //Rules
            services.AddTransient<ICreditCardRule, CardNumberNumeric16Digits>();
            services.AddTransient<ICreditCardRule, Cvv3Numbers>();
            services.AddTransient<ICreditCardRule, ExpiryDateHasntExpired>();
            services.AddTransient<ICreditCardRule, HolderNotEmpty>();
            services.AddTransient<IPaymentAmountRule, NonNegativeAmount>();
            services.AddTransient<ICurrencyExistRule, CurrencyExists>();
            services.AddTransient<IMerchantExistRule, MerchantExists>();

            //Toolbox
            //TODO: Get this from the configuration
            services.AddTransient<ICryptor>(sp => new Cryptor("d09e0b5a-7cb0-4ae5-9598-80ce6a8f0f4b"));
            services.AddSingleton<IGatewayCache, InMemoryGatewayCache>();
            services.AddTransient<IDateTimeProvider, DateTimeProvider>();

            //Services
            services.AddTransient<IPaymentService, PaymentService>();

            services.AddControllers();
            services.AddMvc();
        }

        public void EnsureDatabaseIsSeeded(IApplicationBuilder applicationBuilder)
        {
            // seed the database using an extension method
            using (var serviceScope = applicationBuilder.ApplicationServices
           .GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<PaymentGatewayContext>();
                DatabaseSeeding.Initialize(serviceScope.ServiceProvider);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                EnsureDatabaseIsSeeded(app);
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
