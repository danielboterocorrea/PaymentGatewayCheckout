using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;

namespace AcquiringBank.Simulator
{
    public class Program
    {
        private static string environmentName;
        public static void Main(string[] args)
        {
            Console.Title = "AcquiringBank.Simulator";

            environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (string.IsNullOrEmpty(environmentName))
            {
                throw new ArgumentNullException("Environment ASPNETCORE_ENVIRONMENT variable is missing");
            }

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    if(environmentName == "Development")
                        webBuilder.UseUrls(new[] { "http://*:53677", "https://*:44398" });

                    webBuilder.UseStartup<Startup>();
                });
    }
}
