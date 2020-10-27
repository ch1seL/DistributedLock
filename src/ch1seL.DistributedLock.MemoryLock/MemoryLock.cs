using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace ch1seL.DistributedLock.Memory
{
    public class MemoryLock : IDistributedLock
    {
        private readonly ConcurrentDictionary<string, SemaphoreWrapper> _semaphoreByCacheKeyDictionary =
            new ConcurrentDictionary<string, SemaphoreWrapper>();

        public async Task<IDisposable> CreateLockAsync(string resource, TimeSpan? expiryTime = null,
            TimeSpan? waitTime = null, TimeSpan? retryTime = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            CleanEmpty(resource);
            SemaphoreWrapper semaphore = _semaphoreByCacheKeyDictionary.GetOrAdd(resource, _ => new SemaphoreWrapper());
            return await semaphore.CreateLock(resource, waitTime);
        }

        private void CleanEmpty(string resource)
        {
            foreach (var key in _semaphoreByCacheKeyDictionary
                .Where(s => s.Key != resource)
                .Where(s => s.Value.CurrentCount == 1)
                .Select(s => s.Key))
            {
                _semaphoreByCacheKeyDictionary[key]?.Dispose();
                _semaphoreByCacheKeyDictionary.TryRemove(key, out _);
            }
        }

        private class SemaphoreWrapper : IDisposable
        {
            private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);
            public int CurrentCount => _semaphoreSlim.CurrentCount;

            public void Dispose()
            {
                _semaphoreSlim.Release();
            }

            public async Task<IDisposable> CreateLock(string resource, TimeSpan? waitTime)
            {
                if (await _semaphoreSlim.WaitAsync(waitTime ?? TimeSpan.FromMinutes(1)))
                {
                    return this;    
                }

                throw new DistributedLockException(resource, null, DistributedLockBadStatus.Conflicted);
            }
        }
    }
}