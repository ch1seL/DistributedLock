using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace ch1seL.DistributedLock.Tests.Base
{
    public abstract class LockIntervalTestsBase : IDisposable
    {
        private readonly object _intervalsLock = new object();
        private readonly string _key = Guid.NewGuid().ToString("N");
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private readonly object _taskListLock = new object();

        protected readonly IList<Interval> Intervals = new List<Interval>();
        protected readonly IList<Task> TaskList = new List<Task>();
        private IDistributedLock _distributedLock;
        private ServiceProvider _serviceProvider;

        public void Dispose()
        {
            _serviceProvider?.Dispose();
        }

        protected void Init(Action<IServiceCollection> lockServiceRegistration)
        {
            IServiceCollection services = new ServiceCollection();
            lockServiceRegistration(services);
            _serviceProvider = services.BuildServiceProvider();
            _distributedLock = _serviceProvider.GetRequiredService<IDistributedLock>();
        }

        protected void AddSaveIntervalTaskToTaskList(TimeSpan? waitTime = null, TimeSpan? workTime = null, string key = null)
        {
            Task task = SaveInterval(waitTime, workTime, key);

            lock (_taskListLock)
            {
                TaskList.Add(task);
            }
        }

        private async Task SaveInterval(TimeSpan? waitTime = null, TimeSpan? workTime = null, string key = null)
        {
            using IDisposable lockAsync = await _distributedLock.CreateLockAsync(key ?? _key, TimeSpan.FromMinutes(5), waitTime ?? TimeSpan.FromMinutes(5),
                TimeSpan.FromMilliseconds(100));

            var start = _stopwatch.ElapsedTicks;
            await Task.Delay(workTime ?? TimeSpan.Zero);
            var end = _stopwatch.ElapsedTicks;
            var interval = new Interval(start, end);

            lock (_intervalsLock)
            {
                Intervals.Add(interval);
            }
        }
    }
}