using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace ch1seL.DistributedLock.RedisLock.IntegrationTests.RedisLock
{
    public class RedisLockTestsBase : IDisposable
    {
        private readonly IDistributedLock _distributedLock;
        private readonly string _key = Guid.NewGuid().ToString("N");
        private readonly ServiceProvider _serviceProvider;
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        protected readonly IList<Interval> Intervals = new List<Interval>();

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

        protected async Task AddIntervalTaskWithLock(TimeSpan? waitTime = null, TimeSpan? workTime = null)
        {
            using (await _distributedLock.CreateLockAsync(_key, TimeSpan.FromMinutes(5),
                waitTime ?? TimeSpan.FromMinutes(5), TimeSpan.FromMilliseconds(10)))
            {
                var start = _stopwatch.ElapsedTicks;
                await Task.Delay(workTime ?? TimeSpan.Zero);
                var end = _stopwatch.ElapsedTicks;
                Intervals.Add(new Interval(start, end));
            }
        }
    }
}