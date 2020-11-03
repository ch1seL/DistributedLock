using System;
using System.Threading.Tasks;
using ch1seL.DistributedLock.Tests.Base;
using FluentAssertions;
using Xunit;

namespace ch1seL.DistributedLock.Tests.SharedTests
{
    public class CreateAndAccessAFewKeysDontThrowExceptionsTests : IntervalsWithLockTestsBase
    {
        [Theory]
        [MemberData(nameof(TestsData.LockServiceTypes), MemberType = typeof(TestsData))]
        public async Task Test(Type lockService)
        {
            Init(TestsData.RegistrationByServiceType[lockService]);
            const int repeat = 1000;
            var guids = TestHelpers.GenerateGuidKeys(10);

            Parallel.For(0, repeat, i => { AddSaveIntervalTaskToTaskList(key: guids[i % guids.Length]); });
            Func<Task> act = () => Task.WhenAll(TaskList);

            await act.Should().NotThrowAsync();
            TaskList.Count.Should().Be(repeat);
            Intervals.Count.Should().Be(repeat);
        }
    }
}