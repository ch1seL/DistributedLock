using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace ch1seL.DistributedLock.Tests.Base
{
    public abstract class LockTestsBase:IDisposable
    {
        private IDistributedLock _distributedLock;
        private readonly string _key = Guid.NewGuid().ToString("N");
        private ServiceProvider _serviceProvider;
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        protected readonly IList<Interval> Intervals = new List<Interval>();

        protected void Init(Action<IServiceCollection> lockServiceRegistration)
        {
            IServiceCollection services = new ServiceCollection();
            lockServiceRegistration(services);
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