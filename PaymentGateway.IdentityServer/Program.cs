using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace PaymentGateway.IdentityServer
{
    public class Program
    {
        private static IConfiguration configuration;
        private static string environmentName;

        public static void Main(string[] args)
        {
            Console.Title = "PaymentGateway.IdentityServer";

            environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (string.IsNullOrEmpty(environmentName))
            {
                throw new ArgumentNullException("Environment ASPNETCORE_ENVIRONMENT variable is missing");
            }

            //Getting configuration from appsettings
            configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory))
                .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
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
                .UseSerilog((context, configuration) =>
                {
                    configuration
                        .MinimumLevel.Debug()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .MinimumLevel.Override("System", LogEventLevel.Warning)
                        .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                        .Enrich.FromLogContext()
                        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Literate);
                })
                //.UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    if (environmentName == "Development")
                        webBuilder.UseUrls("https://*:5002", "http://*:5003");

                    webBuilder.UseStartup<Startup>();
                });
    }
}
