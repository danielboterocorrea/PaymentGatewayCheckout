using Microsoft.AspNetCore.Builder;
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
