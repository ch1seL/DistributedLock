using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Xunit;

namespace ch1seL.DistributedLock.RedisLock.IntegrationTests
{
    public class RedisLockTests : RedisLockTestsBase
    {
        private readonly Random _random = new Random();

        [Fact]
        public async Task ParallelsTaskWithLockDontOverlapEachOther()
        {
            const int repeat = 100;
            var intervals = new List<Interval>();
            var stopwatch = Stopwatch.StartNew();
            var key = Guid.NewGuid().ToString("N");

            await Task
                .WhenAll(Enumerable.Repeat((object) null, repeat)
                    .Select(_ => RunTaskWithLock(key, () => AddIntervalTask(intervals, stopwatch))));

            var intersections = intervals
                .SelectMany(interval1 => intervals.Where(interval1.NotEquals).Where(interval1.Intersect)
                    .Select(interval2 => new {interval1, interval2}));

            intervals.Should().HaveCount(repeat);
            intersections.Should().BeEmpty();
        }

        [Fact]
        public async Task ThrowDistributedLockExceptionIfWaitTimeHasExpired()
        {
            var key = Guid.NewGuid().ToString("N");
            
            Func<Task> act = () => Task.WhenAll(
                RunTaskWithLock(key, () => Task.Delay(TimeSpan.FromSeconds(1)), TimeSpan.Zero),
                RunTaskWithLock(key, () => Task.Delay(TimeSpan.FromSeconds(1)), TimeSpan.Zero)
                );

            var exception = await act.Should().ThrowExactlyAsync<DistributedLockException>();
            exception.Which.Resource.Should().Be(key);
            exception.Which.Status.Should().Be(DistributedLockBadStatus.Conflicted);
        }

        private async Task AddIntervalTask(ICollection<Interval> intervals, Stopwatch stopwatch)
        {
            var start = stopwatch.ElapsedTicks;
            await Task.Delay(TimeSpan.FromMilliseconds(_random.Next(100)));
            var end = stopwatch.ElapsedTicks;
            intervals.Add(new Interval(start, end));
        }
    }
}