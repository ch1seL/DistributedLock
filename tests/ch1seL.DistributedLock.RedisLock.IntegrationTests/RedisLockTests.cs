using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
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

            await Task
                .WhenAll(Enumerable.Repeat((object) null, repeat)
                    .Select(_ => RunTaskWithLock(() => AddIntervalTask(intervals, stopwatch))));

            var intersections = intervals
                .SelectMany(interval1 => intervals.Where(interval1.NotEquals).Where(interval1.Intersect)
                    .Select(interval2 => new {interval1, interval2}));

            intervals.Should().HaveCount(repeat);
            intersections.Should().BeEmpty();
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