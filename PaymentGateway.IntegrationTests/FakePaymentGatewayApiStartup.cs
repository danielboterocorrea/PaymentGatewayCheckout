using Microsoft.Extensions.Configuration;
using PaymentGateway.Api;
using Serilog;
using System;
using System.IO;

namespace PaymentGateway.IntegrationTests
{
    public class FakePaymentGatewayApiStartup : Startup
    {
        public FakePaymentGatewayApiStartup(IConfiguration configuration) : base(configuration)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory))
                .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            //Configuring Serilog logging
            Log.Logger = new LoggerConfiguration()
                .ReadFrom
                .Configuration(config)
                .CreateLogger();
        }

        public override string DatabaseName => "PaymentGatewayInMemoryDatabaseTests";

        public override void LaunchConsumer(IServiceProvider serviceProvider)
        {

        }
    }
}
