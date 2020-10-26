using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace ch1seL.DistributedLock.RedisLock.IntegrationTests
{
    public class RedisLockTestsBase : IDisposable
    {
        private readonly IDistributedLock _distributedLock;
        private readonly string _key = Guid.NewGuid().ToString("N");
        private readonly ServiceProvider _serviceProvider;

        protected RedisLockTestsBase()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddStackExchangeRedisLock(options => options.Configuration = "localhost");
            _serviceProvider = services.BuildServiceProvider();

            _distributedLock = _serviceProvider.GetRequiredService<IDistributedLock>();
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
        }

        protected async Task RunTaskWithLock(Func<Task> taskFactory)
        {
            using (await _distributedLock.CreateLockAsync(_key, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5),
                TimeSpan.FromMilliseconds(10)))
            {
                await taskFactory();
            }
        }
    }
}