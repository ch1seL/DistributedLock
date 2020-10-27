using System;
using System.Collections.Generic;
using ch1seL.DistributedLock.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;

namespace ch1seL.DistributedLock.Tests.DistributedLockTests
{
    public class LockServiceImplementationsTestsData
    {
        public static IEnumerable<object[]> LockServiceRegistrations = new[]
        {
            new object[] {nameof(MemoryLock), (Action<IServiceCollection>) (services => services.AddMemoryLock())},
            new object[] {nameof(RedisLock), (Action<IServiceCollection>) (services => services.AddStackExchangeRedisLock(options => options.Configuration = "localhost"))}
        };
    }
}