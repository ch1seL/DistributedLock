using System;
using System.Threading;
using System.Threading.Tasks;
using ch1seL.DistributedLock.Tests.Base;
using FluentAssertions;
using JetBrains.dotMemoryUnit;
using Microsoft.Extensions.Caching.Distributed;
using Xunit;
using Xunit.Abstractions;

namespace ch1seL.DistributedLock.Tests.MemoryLockTests
{
    public class MemoryLeakTests : IntervalsWithLockTestsBase
    {
        public MemoryLeakTests(ITestOutputHelper output)
        {
            DotMemoryUnitTestOutput.SetOutputMethod(output.WriteLine);
            Init(TestsData.RegistrationByServiceType[typeof(MemoryLock)]);
        }

        [Fact]
        [DotMemoryUnit(FailIfRunWithoutSupport = false)]
        public void Test()
        {
            const int repeat = 10;
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var guids = TestHelpers.GenerateGuidKeys(repeat);

            Parallel.For(0, repeat,
                key =>
                {
                    Parallel.For(0, repeat,
                        _ => { AddSaveIntervalTaskToTaskList(key: guids[key], workTime: TimeSpan.FromSeconds(10), cancellationToken: cts.Token); });
                });
            dotMemory.Check(memory =>
            {
                ObjectSet semaphores = memory.GetObjects(where => where.Type.Like(MemoryLock.SemaphoreWrapperTypeFullName));
                semaphores.ObjectsCount.Should().Be(repeat);
            });

            cts.Cancel();

            Action act = () => Task.WhenAll(TaskList).GetAwaiter().GetResult();
            act.Should().Throw<DistributedLockException>();
            Intervals.Count.Should().Be(0);
            dotMemory.Check(memory =>
            {
                ObjectSet semaphores = memory.GetObjects(where => where.Type.Like(MemoryLock.SemaphoreWrapperTypeFullName));
                semaphores.ObjectsCount.Should().Be(0);
            });
        }
    }
}