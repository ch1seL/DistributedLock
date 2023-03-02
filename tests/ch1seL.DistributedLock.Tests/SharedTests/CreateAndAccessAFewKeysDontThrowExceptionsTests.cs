using System;
using System.Threading.Tasks;
using ch1seL.DistributedLock.Tests.Base;
using ch1seL.DistributedLock.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace ch1seL.DistributedLock.Tests.SharedTests;

public abstract class CreateAndAccessAFewKeysDontThrowExceptionsTests : IntervalsWithLockTestsBase {
    private readonly ITestFixture _fixture;

    protected CreateAndAccessAFewKeysDontThrowExceptionsTests(ITestFixture fixture) {
        _fixture = fixture;
    }

    [Fact]
    public async Task Test() {
        Init(_fixture.Registration);
        const int repeat = 1000;
        var guids = TestHelpers.GenerateGuidKeys(10);

        Parallel.For(0, repeat, i => { AddSaveIntervalTaskToTaskList(key: guids[i % guids.Length]); });
        var act = new Func<Task>(() => Task.WhenAll(TaskList));

        await act.Should().NotThrowAsync();
        TaskList.Count.Should().Be(repeat);
        Intervals.Count.Should().Be(repeat);
    }

    [Collection("Redis collection")]
    public class Redis : CreateAndAccessAFewKeysDontThrowExceptionsTests {
        public Redis(RedisFixture fixture) : base(fixture) { }
    }

    [Collection("InMemory collection")]
    public class InMemory : CreateAndAccessAFewKeysDontThrowExceptionsTests {
        public InMemory(InMemoryFixture fixture) : base(fixture) { }
    }
}
