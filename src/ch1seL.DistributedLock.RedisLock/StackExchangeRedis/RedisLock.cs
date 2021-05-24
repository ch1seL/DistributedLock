using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace Microsoft.Extensions.Caching.StackExchangeRedis
{
    public class RedisLock : IDistributedLock, IDisposable
    {
        private readonly SemaphoreSlim _connectionLock = new(1, 1);
        private readonly TimeSpan _defaultExpiryTime = TimeSpan.FromMinutes(2);
        private readonly TimeSpan _defaultRetryTime = TimeSpan.FromMilliseconds(500);
        private readonly TimeSpan _defaultWaitTime = TimeSpan.FromMinutes(1);
        private readonly ILoggerFactory _loggerFactory;
        private readonly RedisLockOptions _options;
        private IDistributedLockFactory _distributedLockFactory;

        public RedisLock(IServiceProvider serviceProvider)
        {
            _options = serviceProvider.GetService<IOptions<RedisLockOptions>>().Value;
            _loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        }

        public void Dispose()
        {
            _connectionLock?.Dispose();
        }

        public async Task<IDisposable> CreateLockAsync(string resource, TimeSpan? expiryTime = null, TimeSpan? waitTime = null, TimeSpan? retryTime = null,
            CancellationToken cancellationToken = default)
        {
            await Connect();

            if (_options.InstanceName != null)
                resource = string.Join('-', _options.InstanceName, resource);

            var @lock = await _distributedLockFactory.CreateLockAsync(resource, expiryTime ?? _defaultExpiryTime, waitTime ?? _defaultWaitTime,
                retryTime ?? _defaultRetryTime, cancellationToken);

            if (@lock.IsAcquired) return @lock;

            var lockId = @lock.LockId;
            var status = RedLockStatusToDistributedLockBadStatus(@lock.Status);
            @lock.Dispose();
            throw new DistributedLockException(resource, lockId, status);
        }

        private static DistributedLockBadStatus RedLockStatusToDistributedLockBadStatus(RedLockStatus redLockStatus)
        {
            return redLockStatus switch
            {
                RedLockStatus.Unlocked => DistributedLockBadStatus.Unlocked,
                RedLockStatus.Conflicted => DistributedLockBadStatus.Conflicted,
                RedLockStatus.Expired => DistributedLockBadStatus.Expired,
                RedLockStatus.NoQuorum => DistributedLockBadStatus.NoQuorum,
                RedLockStatus.Acquired => throw new ArgumentOutOfRangeException(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private async Task Connect()
        {
            if (_distributedLockFactory != null) return;
            await _connectionLock.WaitAsync();
            try
            {
                if (_distributedLockFactory != null) return;
                var connection = _options.ConfigurationOptions != null
                    ? await ConnectionMultiplexer.ConnectAsync(_options.ConfigurationOptions)
                    : await ConnectionMultiplexer.ConnectAsync(_options.Configuration);
                _distributedLockFactory = RedLockFactory.Create(new List<RedLockMultiplexer> {connection}, _loggerFactory);
            }
            finally
            {
                _connectionLock.Release();
            }
        }
    }
}