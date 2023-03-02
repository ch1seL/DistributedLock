using System;
using System.Threading.Tasks;
using ch1seL.DistributedLock.Tests.Base;
using ch1seL.DistributedLock.Tests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Xunit;
using Xunit.Abstractions;

namespace ch1seL.DistributedLock.Tests.SharedTests;

public abstract class ThrowDistributedLockIntervalExceptionIfWaitTimeHasExpiredTests : IntervalsWithLockTestsBase {
    private readonly ITestFixture _fixture;
    private readonly ITestOutputHelper _output;

    protected ThrowDistributedLockIntervalExceptionIfWaitTimeHasExpiredTests(ITestFixture fixture,
        ITestOutputHelper output) {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task Test() {
        Init(collection => _fixture.Registration(collection, _output));


        const int repeat = 10;

        Parallel.For(0, repeat,
            _ => { AddSaveIntervalTaskToTaskList(TimeSpan.FromMilliseconds(10), TimeSpan.FromSeconds(10)); });
        var act = () => Task.WhenAll(TaskList);

        var exception = await act.Should().ThrowExactlyAsync<DistributedLockException>();
        exception.Which.Data["Status"].Should().Be(DistributedLockBadStatus.Conflicted);
        Intervals.Should().HaveCount(1);
    }

    [Collection("Redis collection")]
    public class Redis : ThrowDistributedLockIntervalExceptionIfWaitTimeHasExpiredTests {
        public Redis(RedisFixture fixture, ITestOutputHelper output) : base(fixture, output) { }
    }

    [Collection("InMemory collection")]
    public class InMemory : ThrowDistributedLockIntervalExceptionIfWaitTimeHasExpiredTests {
        public InMemory(InMemoryFixture fixture, ITestOutputHelper output) : base(fixture, output) { }
    }
}