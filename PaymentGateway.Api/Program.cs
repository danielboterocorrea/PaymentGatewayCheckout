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

        private static string environmentName;

        public static void Main(string[] args)
        {
            Console.Title = "PaymentGateway.Api";

            environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

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
                CreateHostBuilder(args, configuration["Ports:http"], configuration["Ports:https"]).Build().Run();
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

        public static IHostBuilder CreateHostBuilder(string[] args, string httpPort, string httpsPort) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    if (environmentName == "Development")
                        webBuilder.UseUrls(new[] { $"http://*:{httpPort}", $"https://*:{httpsPort}" });

                    webBuilder.UseStartup<Startup>();
                });
    }
}
