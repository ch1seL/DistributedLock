using System;
using System.Threading;
using System.Threading.Tasks;
using ch1seL.DistributedLock.Tests.Base;
using ch1seL.DistributedLock.Tests.Fixtures;
using FluentAssertions;
using JetBrains.dotMemoryUnit;
using Microsoft.Extensions.Caching.Distributed;
using Xunit;
using Xunit.Abstractions;

namespace ch1seL.DistributedLock.Tests.MemoryLockTests;

[Collection("InMemory collection")]
public class MemoryLeakTests : IntervalsWithLockTestsBase {
    private readonly InMemoryFixture _fixture;
    private readonly ITestOutputHelper _output;

    public MemoryLeakTests(ITestOutputHelper output, InMemoryFixture fixture) {
        _output = output;
        _fixture = fixture;
        DotMemoryUnitTestOutput.SetOutputMethod(output.WriteLine);
    }

    [Fact]
    [DotMemoryUnit(FailIfRunWithoutSupport = false)]
    public async Task Test() {
        Init(collection => _fixture.Registration(collection, _output));

        const int repeat = 10;
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var guids = TestHelpers.GenerateGuidKeys(repeat);

        Parallel.For(0, repeat,
            key => {
                Parallel.For(0, repeat,
                    _ => {
                        AddSaveIntervalTaskToTaskList(key: guids[key], workTime: TimeSpan.FromSeconds(10),
                            cancellationToken: cts.Token);
                    });
            });
        dotMemory.Check(memory => {
            var semaphores = memory.GetObjects(where => where.Type.Like(MemoryLock.SemaphoreReleaserTypeFullName));
            semaphores.ObjectsCount.Should().Be(repeat);
        });

        await cts.CancelAsync();

        var act = () => Task.WhenAll(TaskList);

        await act.Should().ThrowAsync<DistributedLockException>();
        Intervals.Count.Should().Be(0);
        dotMemory.Check(memory => {
            var semaphores = memory.GetObjects(where => where.Type.Like(MemoryLock.SemaphoreReleaserTypeFullName));
            semaphores.ObjectsCount.Should().Be(0);
        });
    }
}