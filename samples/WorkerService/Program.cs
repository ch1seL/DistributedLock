using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddStackExchangeRedisLock(options => options.Configuration = "localhost");

                    for (var i = 0; i < 5; i++)
                    {
                        services.AddSingleton<IHostedService,Worker>();
                    }
                    
                });
    }
}
