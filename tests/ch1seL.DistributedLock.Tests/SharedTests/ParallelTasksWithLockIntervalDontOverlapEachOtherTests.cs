using System;
using System.Threading.Tasks;
using ch1seL.DistributedLock.Tests.Base;
using ch1seL.DistributedLock.Tests.Fixtures;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace ch1seL.DistributedLock.Tests.SharedTests;

public abstract class ParallelTasksWithLockIntervalDontOverlapEachOtherTests : IntervalsWithLockTestsBase {
    private readonly ITestFixture _fixture;
    private readonly ITestOutputHelper _output;

    protected ParallelTasksWithLockIntervalDontOverlapEachOtherTests(ITestFixture fixture, ITestOutputHelper output) {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task Test() {
        Init(collection => _fixture.Registration(collection, _output));

        const int repeat = 100;

        Parallel.For(0, repeat, _ => { AddSaveIntervalTaskToTaskList(workTime: TimeSpan.FromMilliseconds(10)); });
        await Task.WhenAll(TaskList);

        TaskList.Count.Should().Be(repeat);
        Intervals.Should().HaveCount(repeat);
        GetIntersections().Should().BeEmpty();
    }

    [Collection("Redis collection")]
    public class Redis : ParallelTasksWithLockIntervalDontOverlapEachOtherTests {
        public Redis(RedisFixture fixture, ITestOutputHelper output) : base(fixture, output) { }
    }

    [Collection("InMemory collection")]
    public class InMemory : ParallelTasksWithLockIntervalDontOverlapEachOtherTests {
        public InMemory(InMemoryFixture fixture, ITestOutputHelper output) : base(fixture, output) { }
    }
}