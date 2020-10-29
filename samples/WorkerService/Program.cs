using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WorkerService
{
    public class Program
    {
        public static readonly Guid AppId = Guid.NewGuid();

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args).ConfigureServices((hostContext, services) =>
            {
                if (hostContext.HostingEnvironment.IsDevelopment())
                    services.AddMemoryLock();
                else
                    services.AddStackExchangeRedisLock(options => options.Configuration = "localhost");

                for (var i = 0; i < 5; i++) services.AddSingleton<IHostedService, Worker>();
            });
        }
    }
}