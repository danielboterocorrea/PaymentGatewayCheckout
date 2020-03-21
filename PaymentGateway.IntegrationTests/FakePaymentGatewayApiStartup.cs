﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaymentGateway.Api;
using PaymentGateway.Application.Mapper;
using PaymentGateway.Application.Mappers;
using PaymentGateway.Application.Mappers.Interfaces;
using PaymentGateway.Application.RequestModels;
using PaymentGateway.Application.ResponseModels;
using PaymentGateway.Application.Services;
using PaymentGateway.Application.Services.Interfaces;
using PaymentGateway.Application.Specifications;
using PaymentGateway.Application.Toolbox.Interfaces;
using PaymentGateway.Domain.Metrics;
using PaymentGateway.Domain.Repositories;
using PaymentGateway.Domain.Specifications;
using PaymentGateway.Domain.Toolbox;
using PaymentGateway.Domain.Toolbox.Interfaces;
using PaymentGateway.Domain.Validators;
using PaymentGateway.Infrastructure;
using PaymentGateway.Infrastructure.DatabaseModels;
using PaymentGateway.Infrastructure.Metrics;
using PaymentGateway.Infrastructure.Repositories;
using PaymentGateway.Infrastructure.Repositories.Cache;
using PaymentGateway.Infrastructure.Toolbox;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;

namespace PaymentGateway.IntegrationTests
{
    public class FakePaymentGatewayApiStartup : Startup
    {
        public FakePaymentGatewayApiStartup(IConfiguration configuration) : base(configuration)
        {
            //Configuring Serilog logging
            Log.Logger = new LoggerConfiguration()
                .ReadFrom
                .Configuration(configuration)
                .CreateLogger();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddHttpContextAccessor();
            services.AddMemoryCache();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
            });

            services.AddDbContext<PaymentGatewayContext>(optionsBuilder =>
                   optionsBuilder.UseInMemoryDatabase(databaseName: "PaymentGatewayInMemoryDatabaseTests"));

            ConfigureRepositories(services);
            ConfigureRules(services);
            ConfigureToolbox(services);
            ConfigureAppServices(services);
            ConfigureMetrics(services);

            services.AddControllers()
                    .AddNewtonsoftJson();

            //TODO: Configuration file
            string authority = "https://localhost:5002";

            ConfigureIdentityServer(services, authority);

        }

        private static void ConfigureMetrics(IServiceCollection services)
        {
            //Metrics
            services.AddSingleton<IMetricsCounter, PrometheusMetricsCounter>();
            services.AddSingleton<IMetricsTime, PrometheusMetricsTime>();
        }

        private static void ConfigureAppServices(IServiceCollection services)
        {
            //Services
            services.AddTransient<IPaymentService, PaymentService>();
        }

        private static void ConfigureToolbox(IServiceCollection services)
        {

            //Toolbox
            //TODO: Get this from the configuration
            services.AddTransient<ICryptor>(sp => new Cryptor("d09e0b5a-7cb0-4ae5-9598-80ce6a8f0f4b"));
            services.AddSingleton<IGatewayCache, InMemoryGatewayCache>();
            services.AddTransient<IDateTimeProvider, DateTimeProvider>();
            services.AddSingleton<IProducerConsumer<PaymentRequest>>(ps =>
            {
                return GetProducerConsumerSender<PaymentRequest, AcquiringBankPaymentResponse>(ps);
            });
        }

        private static ProducerConsumerSender<T, R> GetProducerConsumerSender<T, R>(IServiceProvider sp) where T : IGetId
        {
            var loggerFactory = (ILoggerFactory)sp.GetService(typeof(ILoggerFactory));
            var httpClientFactory = (IHttpClientFactory)sp.GetService(typeof(IHttpClientFactory));
            var httpClient = httpClientFactory.CreateClient();
            var acquiringBankPaymentService = new AcquiringBankPaymentService(loggerFactory
                .CreateLogger<AcquiringBankPaymentService>(), httpClient);
            var timeOutHttpRequest = new TimeoutRequest<T, R>((ISendItem<T, R>)acquiringBankPaymentService,
                loggerFactory.CreateLogger<TimeoutRequest<T, R>>(), TimeSpan.FromSeconds(5));
            var retries = new RetryRequest<T, R>(timeOutHttpRequest,
                loggerFactory.CreateLogger<RetryRequest<T, R>>(), 3);
            return new ProducerConsumerSender<T, R>(loggerFactory.CreateLogger<ProducerConsumerSender<T, R>>(), 3,
                retries);
        }

        private static void ConfigureRules(IServiceCollection services)
        {
            //Rules
            services.AddTransient<ICreditCardRule, CardNumberNumeric16Digits>();
            services.AddTransient<ICreditCardRule, Cvv3Numbers>();
            services.AddTransient<ICreditCardRule, ExpiryDateHasntExpired>();
            services.AddTransient<ICreditCardRule, HolderNotEmpty>();
            services.AddTransient<IPaymentAmountRule, NonNegativeAmount>();
            services.AddTransient<ICurrencyExistRule, CurrencyExists>();
            services.AddTransient<IMerchantExistRule, MerchantExists>();
        }

        private static void ConfigureRepositories(IServiceCollection services)
        {
            //Repositories
            services.AddTransient<CurrencyRepository>();
            services.AddTransient<MerchantRepository>();
            services.AddTransient<PaymentRepository>();
            services.AddTransient<ICurrencyRepository, CurrencyCacheRepository>();
            services.AddTransient<IMerchantRepository, MerchantCacheRepository>();
            services.AddTransient<IPaymentRepository, PaymentCacheRepository>();
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
        }

        private static void ConfigureIdentityServer(IServiceCollection services, string authority)
        {

            //IdentityServer4
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    //TODO: Get this from the configuration
                    options.Authority = authority;
                    options.RequireHttpsMetadata = true;

                    options.Audience = "PaymentGatewayApi";
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public override void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            EnsureDatabaseIsSeeded(app);

            //Do not redirect
            //app.UseHttpsRedirection();

            app.UseRouting();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
