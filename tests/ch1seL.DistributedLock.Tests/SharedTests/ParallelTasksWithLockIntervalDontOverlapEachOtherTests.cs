using System;
using System.Linq;
using System.Threading.Tasks;
using ch1seL.DistributedLock.Tests.Base;
using FluentAssertions;
using Xunit;

namespace ch1seL.DistributedLock.Tests.SharedTests
{
    public class ParallelTasksWithLockIntervalDontOverlapEachOtherTests : LockIntervalTestsBase
    {
        [Theory]
        [MemberData(nameof(TestsData.LockServiceTypes), MemberType = typeof(TestsData))]
        public async Task Test(Type lockServiceType)
        {
            Init(TestsData.RegistrationByServiceType[lockServiceType]);
            const int repeat = 100;

            Parallel.For(0, repeat, _ => { AddSaveIntervalTaskToTaskList(); });
            await Task.WhenAll(TaskList);
            var intersections = Intervals.SelectMany(interval1 =>
                Intervals.Where(interval1.NotEquals).Where(interval1.Intersect).Select(interval2 => new {interval1, interval2}));

            TaskList.Count.Should().Be(repeat);
            Intervals.Should().HaveCount(repeat);
            intersections.Should().BeEmpty();
        }
    }
}