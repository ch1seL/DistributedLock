using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace ch1seL.DistributedLock.Tests.Base
{
    public abstract class IntervalsWithLockTestsBase : IDisposable
    {
        private readonly object _intervalsLock = new();
        private readonly string _key = Guid.NewGuid().ToString("N");
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private readonly object _taskListLock = new();

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

        protected void AddSaveIntervalTaskToTaskList(TimeSpan? waitTime = null, TimeSpan? workTime = null, string key = null,
            CancellationToken cancellationToken = default)
        {
            var task = SaveInterval(waitTime, workTime, key, cancellationToken);

            lock (_taskListLock)
            {
                TaskList.Add(task);
            }
        }

        private async Task SaveInterval(TimeSpan? waitTime = null, TimeSpan? workTime = null, string key = null, CancellationToken cancellationToken = default)
        {
            using var lockAsync = await _distributedLock.CreateLockAsync(key ?? _key, TimeSpan.FromMinutes(5), waitTime ?? TimeSpan.FromMinutes(5),
                TimeSpan.FromMilliseconds(100), cancellationToken);

            var start = _stopwatch.ElapsedTicks;
            await Task.Delay(workTime ?? TimeSpan.Zero, cancellationToken);
            var end = _stopwatch.ElapsedTicks;
            var interval = new Interval(start, end);

            lock (_intervalsLock)
            {
                Intervals.Add(interval);
            }
        }

        protected IEnumerable<(Interval, Interval)> GetIntersections()
        {
            lock (_intervalsLock)
            {
                return Intervals.SelectMany(interval1 =>
                        Intervals.Where(interval1.NotEquals).Where(interval1.Intersect).Select(interval2 => (interval1, interval2)))
                    .ToArray();
            }
        }
    }
}