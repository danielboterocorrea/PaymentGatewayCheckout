using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace AcquiringBank.Simulator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                    .UseUrls(new[] { "http://*:53677", "https://*:44398" })
                    .UseStartup<Startup>();
                });
    }
}
