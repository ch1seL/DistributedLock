﻿using System;
using System.Linq;
using System.Threading.Tasks;
using ch1seL.DistributedLock.Tests.Base;
using FluentAssertions;
using Xunit;

namespace ch1seL.DistributedLock.Tests.DistributedLockTests
{
    public class CreateAndAccessRandomKeysDontThrowExceptionsTests : LockTestsBase
    {
        [Theory]
        [MemberData(nameof(LockServiceImplementationsTestsData.LockServiceTypes),
            MemberType = typeof(LockServiceImplementationsTestsData))]
        public async Task Test(Type lockService)
        {
            Init(LockServiceImplementationsTestsData.RegistrationByServiceType[lockService]);
            const int repeat = 100;

            Func<Task> act = async () => await Task
                .WhenAll(Enumerable.Repeat((object) null, repeat)
                    .SelectMany(_ => Enumerable.Repeat(Guid.NewGuid().ToString("N"), repeat)
                        .Select(guid => AddIntervalTaskWithLock(key: guid))));

            await act.Should().NotThrowAsync();
        }
    }
}