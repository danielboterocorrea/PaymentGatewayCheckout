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
        }

        public override void LaunchConsumer(IServiceProvider serviceProvider)
        {

        }
    }
}
