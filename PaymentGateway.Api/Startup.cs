using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
using Microsoft.OpenApi.Models;
using Prometheus;
using PaymentGateway.Domain.Metrics;
using PaymentGateway.Infrastructure.Metrics;
using PaymentGateway.Application.Toolbox.Interfaces;
using System.Net.Http;
using PaymentGateway.Application.RequestModels;
using PaymentGateway.Application.ResponseModels;
using System.Threading.Tasks;

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
        public virtual void ConfigureServices(IServiceCollection services)
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
                   optionsBuilder.UseInMemoryDatabase(databaseName: "PaymentGatewayInMemoryDatabase"));

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

            ConfigureSwagger(services, authority);
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
            services.AddSingleton<IPublisher<PaymentRequest>, InMemoryQueue<PaymentRequest>>();
            services.AddTransient<IPaymentService, PaymentService>();
            services.AddTransient(sp =>
            {
                var loggerFactory = (ILoggerFactory)sp.GetService(typeof(ILoggerFactory));
                var cryptor = (ICryptor)sp.GetService(typeof(ICryptor));
                var httpClientFactory = (IHttpClientFactory)sp.GetService(typeof(IHttpClientFactory));
                var gatewayCache = (IGatewayCache)sp.GetService(typeof(IGatewayCache));
                //var paymentRepository = (IPaymentRepository)sp.GetService(typeof(IPaymentRepository));
                var paymentRepository = new PaymentCacheRepository(new PaymentRepository(cryptor, GetLongRunningContext()),
                    loggerFactory.CreateLogger<PaymentCacheRepository>(), gatewayCache);
                var httpClient = httpClientFactory.CreateClient();
                return new AcquiringBankPaymentService(loggerFactory.CreateLogger<AcquiringBankPaymentService>(),
                    httpClient, paymentRepository);
            });
        }

        private static PaymentGatewayContext PaymentGatewayContextLongRunning = null;
        public static PaymentGatewayContext GetLongRunningContext()
        {
            if(PaymentGatewayContextLongRunning == null)
            {
                var optionsBuilder = new DbContextOptionsBuilder<PaymentGatewayContext>();
                optionsBuilder.UseInMemoryDatabase(databaseName: "PaymentGatewayInMemoryDatabase");
                PaymentGatewayContextLongRunning = new PaymentGatewayContext(optionsBuilder.Options);
            }
            return PaymentGatewayContextLongRunning;
        }

        private static void ConfigureToolbox(IServiceCollection services)
        {
            //Toolbox
            //TODO: Get this from the configuration
            services.AddTransient<ICryptor>(sp => new Cryptor("d09e0b5a-7cb0-4ae5-9598-80ce6a8f0f4b"));
            services.AddSingleton<IGatewayCache, InMemoryGatewayCache>();
            services.AddTransient<IDateTimeProvider, DateTimeProvider>();
            //services.AddScoped(sp =>
            //{
            //    return GetProducerConsumerSender<PaymentRequest, AcquiringBankPaymentResponse>(sp);
            //});

        }

        private static ISendItem<T, R> GetSendPayment<T, R>(IServiceProvider sp) where T : IGetId
        {
            var loggerFactory = (ILoggerFactory)sp.GetService(typeof(ILoggerFactory));
            var acquiringBankPaymentService = (AcquiringBankPaymentService)sp.GetService(typeof(AcquiringBankPaymentService));
            var timeOutHttpRequest = new TimeoutRequest<T, R>((ISendItem<T, R>)acquiringBankPaymentService,
                loggerFactory.CreateLogger<TimeoutRequest<T, R>>(), TimeSpan.FromSeconds(5));
            var retries = new RetryRequest<T, R>(timeOutHttpRequest,
                loggerFactory.CreateLogger<RetryRequest<T, R>>(), 3);

            return retries;
        }

        private static ConsumerSender<T,R> GetProducerConsumerSender<T, R>(IServiceProvider sp) where T : IGetId
        {
            var loggerFactory = (ILoggerFactory)sp.GetService(typeof(ILoggerFactory));
            var retries = GetSendPayment<T, R>(sp);
            var inMemoryQueue = (IQueueProvider<T>)sp.GetService(typeof(IPublisher<T>));
            return new ConsumerSender<T, R>(loggerFactory.CreateLogger<ConsumerSender<T, R>>(), 3,
                retries, inMemoryQueue);
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

        private static void ConfigureSwagger(IServiceCollection services, string authority)
        {
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PaymentGateway API", Version = "v1" });
                c.AddSecurityDefinition("ResourceOwner", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Query,
                    OpenIdConnectUrl = new Uri($"{authority}/.well-known/openid-configuration"),
                    Name = "Authorization",
                    Type = SecuritySchemeType.OAuth2,
                    Scheme = "bearer",
                    Flows = new OpenApiOAuthFlows
                    {
                        Password = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri($"{authority}/connect/authorize", UriKind.Absolute),
                            TokenUrl = new Uri($"{authority}/connect/token", UriKind.Absolute),
                            Scopes = new Dictionary<string, string>
                            {
                                { "PaymentGatewayApi", "PaymentGatewayApi" }
                            }
                        }
                    }
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement{
                {
                    new OpenApiSecurityScheme{
                        Reference = new OpenApiReference{
                            Id = "ResourceOwner", //The name of the previously defined security scheme.
                            Type = ReferenceType.SecurityScheme
                        }
                    },new List<string>()
                }});
            });
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
        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            //
            Task.Run(() => {
                var consumer = GetProducerConsumerSender<PaymentRequest, AcquiringBankPaymentResponse>(serviceProvider);
                var task = consumer.ConsumeAsync();
                Task.WaitAll(task);
            });

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "PaymentGatewayApi V1");
                c.RoutePrefix = string.Empty;
                c.OAuthClientId("SwaggerApi");
                c.OAuthClientSecret("7da3e461-a80e-4e02-a968-e21e255c4ec6");
                c.OAuthAppName("PaymentGateway Api");
                c.OAuth2RedirectUrl("https://localhost:44346/index.html");
                c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
            });

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
            app.UseMetricServer();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
