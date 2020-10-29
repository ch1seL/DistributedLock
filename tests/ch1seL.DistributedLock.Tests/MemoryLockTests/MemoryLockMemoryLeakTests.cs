using System;
using System.Threading.Tasks;
using ch1seL.DistributedLock.Tests.Base;
using FluentAssertions;
using JetBrains.dotMemoryUnit;
using Microsoft.Extensions.Caching;
using Xunit;
using Xunit.Abstractions;

namespace ch1seL.DistributedLock.Tests.MemoryLockTests
{
    public class MemoryLockMemoryLeakTests : IntervalsWithLockTestsBase
    {
        public MemoryLockMemoryLeakTests(ITestOutputHelper output)
        {
            DotMemoryUnitTestOutput.SetOutputMethod(output.WriteLine);
            Init(TestsData.RegistrationByServiceType[typeof(MemoryLock)]);
        }

        [Fact]
        [DotMemoryUnit(FailIfRunWithoutSupport = false)]
        public async Task Test()
        {
            const int repeat = 100;

            Parallel.For(0, repeat, i => AddSaveIntervalTaskToTaskList(key: i.ToString()));
            Func<Task> act = () => Task.WhenAll(TaskList);
            ;

            await act.Should().NotThrowAsync();
            Intervals.Count.Should().Be(repeat);
            dotMemory.Check(memory =>
            {
                ObjectSet semaphores = memory.GetObjects(where => where.Type.Is<MemoryLock.SemaphoreWrapper>());
                semaphores.ObjectsCount.Should().Be(0);
                semaphores.SizeInBytes.Should().Be(0);
            });
        }
    }
}