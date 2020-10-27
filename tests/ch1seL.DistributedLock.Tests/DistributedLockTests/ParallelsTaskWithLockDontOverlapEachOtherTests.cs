﻿using System;
using System.Linq;
using System.Threading.Tasks;
using ch1seL.DistributedLock.Tests.Base;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ch1seL.DistributedLock.Tests.DistributedLockTests
{
    public class ParallelsTaskWithLockDontOverlapEachOtherTests : LockTestsBase
    {
        private readonly Random _random = new Random();

        [Theory]
        [MemberData(nameof(LockServiceImplementationsTestsData.LockServiceRegistrations), MemberType = typeof(LockServiceImplementationsTestsData))]
        public async Task Test(string lockService, Action<IServiceCollection> lockServiceRegistration)
        {
            Init(lockServiceRegistration);

            const int repeat = 100;

            await Task
                .WhenAll(Enumerable.Repeat((object) null, repeat)
                    .Select(__ => AddIntervalTaskWithLock(workTime: TimeSpan.FromMilliseconds(_random.Next(10)))));

            var intersections = Intervals
                .SelectMany(interval1 => Intervals.Where(interval1.NotEquals).Where(interval1.Intersect)
                    .Select(interval2 => new {interval1, interval2}));

            Intervals.Should().HaveCount(repeat);
            intersections.Should().BeEmpty();
        }
    }
}