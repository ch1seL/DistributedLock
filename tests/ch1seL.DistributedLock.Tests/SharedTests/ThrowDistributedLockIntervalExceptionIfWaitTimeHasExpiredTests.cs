using System;
using System.Threading.Tasks;
using ch1seL.DistributedLock.Tests.Base;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Xunit;

namespace ch1seL.DistributedLock.Tests.SharedTests
{
    public class ThrowDistributedLockIntervalExceptionIfWaitTimeHasExpiredTests : LockIntervalTestsBase
    {
        [Theory]
        [MemberData(nameof(TestsData.LockServiceTypes),
            MemberType = typeof(TestsData))]
        public async Task Test(Type lockServiceType)
        {
            Init(TestsData.RegistrationByServiceType[lockServiceType]);
            const int repeat = 10;

            Parallel.For(0, repeat,
                _ => { AddSaveIntervalTaskToTaskList(TimeSpan.FromMilliseconds(10), TimeSpan.FromSeconds(10)); });
            Func<Task> act = () => Task.WhenAll(TaskList);

            var exception = await act.Should().ThrowExactlyAsync<DistributedLockException>();
            exception.Which.Status.Should().Be(DistributedLockBadStatus.Conflicted);
            Intervals.Should().HaveCount(1);
        }
    }
}