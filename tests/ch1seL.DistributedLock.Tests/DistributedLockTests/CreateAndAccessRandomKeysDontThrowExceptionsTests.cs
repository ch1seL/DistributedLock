using System;
using System.Linq;
using System.Threading.Tasks;
using ch1seL.DistributedLock.Tests.Base;
using FluentAssertions;
using Xunit;

namespace ch1seL.DistributedLock.Tests.DistributedLockTests
{
    public class CreateAndAccessRandomKeysDontThrowExceptionsTests : LockTestsBase
    {
        private readonly Random _random = new Random();

        [Theory]
        [MemberData(nameof(LockServiceImplementationsTestsData.LockServiceTypes),
            MemberType = typeof(LockServiceImplementationsTestsData))]
        public async Task Test(Type lockService)
        {
            Init(LockServiceImplementationsTestsData.RegistrationByServiceType[lockService]);
            const int repeat = 100;

            Func<Task> act = async () => await Task
                .WhenAll(Enumerable.Repeat<Func<string>>(()=>Guid.NewGuid().ToString("N"), repeat)
                    .SelectMany(getKeyFunc => Enumerable.Repeat((object) null, repeat).Select(___ =>
                        AddIntervalTaskWithLock(workTime: TimeSpan.FromMilliseconds(_random.Next(10)),
                            key: getKeyFunc()))));

            await act.Should().NotThrowAsync();
        }
    }
}