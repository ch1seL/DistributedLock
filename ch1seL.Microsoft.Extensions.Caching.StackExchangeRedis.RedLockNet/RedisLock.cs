using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace Microsoft.Extensions.Caching
{
    public class RedisLock : IDistributedLock, IDisposable
    {
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(1, 1);
        private readonly TimeSpan _defaultExpiryTime = TimeSpan.FromSeconds(60);
        private readonly TimeSpan _defaultRetryTime = TimeSpan.FromMilliseconds(500);
        private readonly TimeSpan _defaultWaitTime = TimeSpan.FromSeconds(30);
        private readonly RedisCacheOptions _options;
        private IDistributedLockFactory _distributedLockFactory;

        public RedisLock(IOptions<RedisCacheOptions> redisCacheOptionsAccessor)
        {
            _options = redisCacheOptionsAccessor.Value;
        }

        public void Dispose()
        {
            _connectionLock?.Dispose();
        }

        public async Task<IDisposable> CreateLockAsync(string resource, TimeSpan? expiryTime = null,
            TimeSpan? waitTime = null,
            TimeSpan? retryTime = null,
            CancellationToken cancellationToken = default)
        {
            await Connect();

            if (_options.InstanceName != null)
                resource = string.Join('-', _options.InstanceName ?? string.Empty, resource);

            IRedLock @lock = await _distributedLockFactory.CreateLockAsync(resource, expiryTime ?? _defaultExpiryTime,
                waitTime ?? _defaultWaitTime, retryTime ?? _defaultRetryTime, cancellationToken);
            if (@lock.IsAcquired) return @lock;

            var lockId = @lock.LockId;
            RedLockStatus status = @lock.Status;
            @lock.Dispose();
            throw new RedisLockException(resource, lockId, status);
        }

        private async Task Connect()
        {
            await _connectionLock.WaitAsync();
            try
            {
                if (_distributedLockFactory == null)
                {
                    ConnectionMultiplexer connection = _options.ConfigurationOptions != null
                        ? await ConnectionMultiplexer.ConnectAsync(_options.ConfigurationOptions)
                        : await ConnectionMultiplexer.ConnectAsync(_options.Configuration);
                    _distributedLockFactory = RedLockFactory.Create(new List<RedLockMultiplexer> {connection});
                }
            }
            finally
            {
                _connectionLock.Release();
            }
        }
    }
}