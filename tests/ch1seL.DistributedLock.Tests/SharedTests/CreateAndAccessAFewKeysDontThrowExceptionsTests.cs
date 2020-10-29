using System;
using System.Threading.Tasks;
using ch1seL.DistributedLock.Tests.Base;
using FluentAssertions;
using Xunit;

namespace ch1seL.DistributedLock.Tests.SharedTests
{
    public class CreateAndAccessAFewKeysDontThrowExceptionsTests : LockIntervalTestsBase
    {
        [Theory]
        [MemberData(nameof(TestsData.LockServiceTypes),
            MemberType = typeof(TestsData))]
        public async Task Test(Type lockService)
        {
            Init(TestsData.RegistrationByServiceType[lockService]);
            const int repeat = 100;

            Parallel.For(0, repeat,
                key => { Parallel.For(0, repeat, _ => { AddSaveIntervalTaskToTaskList(key: key.ToString()); }); });
            Func<Task> act = () => Task.WhenAll(TaskList);

            await act.Should().NotThrowAsync();
            TaskList.Count.Should().Be(repeat * repeat);
            Intervals.Count.Should().Be(repeat * repeat);
        }
    }
}