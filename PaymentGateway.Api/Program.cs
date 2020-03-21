using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace PaymentGateway.Api
{
    public class Program
    {
        private static IConfiguration configuration;

        public static void Main(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (string.IsNullOrEmpty(environmentName))
            {
                throw new ArgumentNullException("Environment ASPNETCORE_ENVIRONMENT variable is missing");
            }

            //Getting configuration from appsettings
            configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory))
                .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: false, reloadOnChange: true)
                .Build();

            //Configuring Serilog logging
            Log.Logger = new LoggerConfiguration()
                .ReadFrom
                .Configuration(configuration)
                .CreateLogger();

            try
            {
                Log.Information("Starting up");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                    .UseUrls(new[] { "http://*:53746", "https://*:44346" })
                    .UseStartup<Startup>();
                });
    }
}
