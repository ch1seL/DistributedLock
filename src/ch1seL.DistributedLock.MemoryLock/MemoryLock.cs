using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Microsoft.Extensions.Caching
{
    public class MemoryLock : IDistributedLock
    {
        private readonly ConcurrentDictionary<string, SemaphoreWrapper> _semaphoreByCacheKeyDictionary =
            new ConcurrentDictionary<string, SemaphoreWrapper>();

        public async Task<IDisposable> CreateLockAsync(string resource, TimeSpan? expiryTime = null,
            TimeSpan? waitTime = null, TimeSpan? retryTime = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            DisposeNotUsedSemaphores(resource);
            SemaphoreWrapper semaphoreWrapper =
                _semaphoreByCacheKeyDictionary.GetOrAdd(resource, _ => new SemaphoreWrapper());
            return await semaphoreWrapper.CreateLock(resource, waitTime);
        }

        private void DisposeNotUsedSemaphores(string resource)
        {
            foreach (SemaphoreWrapper semaphoreWrapper in _semaphoreByCacheKeyDictionary
                .Where(s => s.Key != resource && s.Value.CurrentCount == 1)
                .Select(s =>
                    _semaphoreByCacheKeyDictionary.TryRemove(s.Key, out SemaphoreWrapper semaphoreToDispose)
                        ? semaphoreToDispose
                        : null)
            )
                semaphoreWrapper?.DisposeSemaphore();
        }

        private class SemaphoreWrapper : IDisposable
        {
            private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);
            public int CurrentCount => _semaphoreSlim.CurrentCount;

            public void Dispose()
            {
                _semaphoreSlim.Release();
            }

            public void DisposeSemaphore()
            {
                _semaphoreSlim?.Dispose();
            }

            public async Task<IDisposable> CreateLock(string resource, TimeSpan? waitTime)
            {
                if (await _semaphoreSlim.WaitAsync(waitTime ?? TimeSpan.FromMinutes(1))) return this;

                throw new DistributedLockException(resource, null, DistributedLockBadStatus.Conflicted);
            }
        }
    }
}