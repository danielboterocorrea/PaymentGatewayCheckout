using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;

namespace AcquiringBank.Simulator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "AcquiringBank.Simulator";

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                    .UseUrls(new[] { "http://192.168.1.76:53677", "https://192.168.1.76:44398" })
                    .UseStartup<Startup>();
                });
    }
}
