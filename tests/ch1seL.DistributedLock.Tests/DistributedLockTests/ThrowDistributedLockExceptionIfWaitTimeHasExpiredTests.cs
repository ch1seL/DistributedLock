using System;
using System.Threading.Tasks;
using ch1seL.DistributedLock.Tests.Base;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ch1seL.DistributedLock.Tests.DistributedLockTests
{
    public class ThrowDistributedLockExceptionIfWaitTimeHasExpiredTests : LockTestsBase
    {
        [Theory]
        [MemberData(nameof(LockServiceImplementationsTestsData.LockServiceRegistrations), MemberType = typeof(LockServiceImplementationsTestsData))]
        public async Task Test(string lockService, Action<IServiceCollection> lockServiceRegistration)
        {
            Init(lockServiceRegistration);

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