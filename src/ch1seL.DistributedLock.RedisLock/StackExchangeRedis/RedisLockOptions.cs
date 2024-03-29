﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Microsoft.Extensions.Caching.StackExchangeRedis; 

/// <summary>
///     Configuration options for <see cref="Microsoft.Extensions.Caching.StackExchangeRedis.RedisLock" />.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class RedisLockOptions : IOptions<RedisLockOptions> {
    /// <summary>
    ///     The configuration used to connect to Redis.
    /// </summary>
    public string Configuration { get; set; }

    /// <summary>
    ///     The configuration used to connect to Redis.
    ///     This is preferred over Configuration.
    /// </summary>
    public ConfigurationOptions ConfigurationOptions { get; set; }

    /// <summary>
    ///     The Redis instance name.
    /// </summary>
    public string InstanceName { get; set; }

    RedisLockOptions IOptions<RedisLockOptions>.Value => this;
}
