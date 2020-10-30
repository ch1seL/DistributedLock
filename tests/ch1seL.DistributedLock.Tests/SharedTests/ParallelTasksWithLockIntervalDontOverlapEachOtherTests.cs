using System;
using System.Threading.Tasks;
using ch1seL.DistributedLock.Tests.Base;
using FluentAssertions;
using Xunit;

namespace ch1seL.DistributedLock.Tests.SharedTests
{
    public class ParallelTasksWithLockIntervalDontOverlapEachOtherTests : IntervalsWithLockTestsBase
    {
        [Theory]
        [MemberData(nameof(TestsData.LockServiceTypes), MemberType = typeof(TestsData))]
        public async Task Test(Type lockServiceType)
        {
            Init(TestsData.RegistrationByServiceType[lockServiceType]);
            const int repeat = 100;

            Parallel.For(0, repeat, _ => { AddSaveIntervalTaskToTaskList(workTime: TimeSpan.FromMilliseconds(10)); });
            await Task.WhenAll(TaskList);

            TaskList.Count.Should().Be(repeat);
            Intervals.Should().HaveCount(repeat);
            GetIntersections().Should().BeEmpty();
        }
    }
}