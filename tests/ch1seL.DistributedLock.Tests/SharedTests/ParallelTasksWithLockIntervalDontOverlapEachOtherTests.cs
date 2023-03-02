using System;
using System.Threading.Tasks;
using ch1seL.DistributedLock.Tests.Base;
using ch1seL.DistributedLock.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace ch1seL.DistributedLock.Tests.SharedTests;

public abstract class ParallelTasksWithLockIntervalDontOverlapEachOtherTests : IntervalsWithLockTestsBase {
    private readonly ITestFixture _fixture;

    protected ParallelTasksWithLockIntervalDontOverlapEachOtherTests(ITestFixture fixture) {
        _fixture = fixture;
    }

    [Fact]
    public async Task Test() {
        Init(_fixture.Registration);
        const int repeat = 100;

        Parallel.For(0, repeat, _ => { AddSaveIntervalTaskToTaskList(workTime: TimeSpan.FromMilliseconds(10)); });
        await Task.WhenAll(TaskList);

        TaskList.Count.Should().Be(repeat);
        Intervals.Should().HaveCount(repeat);
        GetIntersections().Should().BeEmpty();
    }

    [Collection("Redis collection")]
    public class Redis : ParallelTasksWithLockIntervalDontOverlapEachOtherTests {
        public Redis(RedisFixture fixture) : base(fixture) { }
    }

    [Collection("InMemory collection")]
    public class InMemory : ParallelTasksWithLockIntervalDontOverlapEachOtherTests {
        public InMemory(InMemoryFixture fixture) : base(fixture) { }
    }
}
