using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.IO;

namespace PaymentGateway.IntegrationTests
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory))
                .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.Testing.json", optional: false, reloadOnChange: true)
                .Build();

            return WebHost.CreateDefaultBuilder(null)
                            .UseEnvironment("Testing")
                            .UseConfiguration(configuration)
                            .UseStartup<TStartup>();
        }
    }
}
