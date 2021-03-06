﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;

namespace ch1seL.DistributedLock.Tests
{
    public class TestsData
    {
        public static IEnumerable<object[]> LockServiceTypes = new[]
        {
            new object[] {typeof(RedisLock)},
            new object[] {typeof(MemoryLock)}
        };

        public static readonly IReadOnlyDictionary<Type, Action<IServiceCollection>> RegistrationByServiceType =
            new Dictionary<Type, Action<IServiceCollection>>
            {
                {
                    typeof(MemoryLock),
                    services => services.AddMemoryLock()
                },
                {
                    typeof(RedisLock),
                    services => services.AddStackExchangeRedisLock(options => options.Configuration = "localhost")
                }
            };
    }
}