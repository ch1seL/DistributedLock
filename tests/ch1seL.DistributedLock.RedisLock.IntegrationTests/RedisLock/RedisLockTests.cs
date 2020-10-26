using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Xunit;

namespace ch1seL.DistributedLock.RedisLock.IntegrationTests.RedisLock
{
    public class RedisLockTests
    {
        public class ParallelsTaskWithLockDontOverlapEachOther : RedisLockTestsBase
        {
            private readonly Random _random = new Random();

            [Fact]
            public async Task Test()
            {
                const int repeat = 100;

                await Task
                    .WhenAll(Enumerable.Repeat((object) null, repeat)
                        .Select(_ => AddIntervalTaskWithLock(workTime: TimeSpan.FromMilliseconds(_random.Next(10)))));

                var intersections = Intervals
                    .SelectMany(interval1 => Intervals.Where(interval1.NotEquals).Where(interval1.Intersect)
                        .Select(interval2 => new {interval1, interval2}));

                Intervals.Should().HaveCount(repeat);
                intersections.Should().BeEmpty();
            }
        }
        
        public class ThrowDistributedLockExceptionIfWaitTimeHasExpired : RedisLockTestsBase
        {
            [Fact]
            public async Task Test()
            {
                Func<Task> act = () => Task.WhenAll(
                    AddIntervalTaskWithLock(TimeSpan.Zero, TimeSpan.FromSeconds(1)),
                    AddIntervalTaskWithLock(TimeSpan.Zero, TimeSpan.FromSeconds(1))
                );
            
                var exception = await act.Should().ThrowExactlyAsync<DistributedLockException>();
                exception.Which.Status.Should().Be(DistributedLockBadStatus.Conflicted);
                Intervals.Should().HaveCount(1);
            }
        }
    }
}