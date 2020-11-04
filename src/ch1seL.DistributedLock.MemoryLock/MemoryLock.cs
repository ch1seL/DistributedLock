using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Caching.Distributed
{
    public class MemoryLock : IDistributedLock, IDisposable
    {
        public static readonly string SemaphoreReleaserTypeFullName = typeof(SemaphoreReleaser).FullName;

        private readonly ConcurrentDictionary<string, RefCounted<SemaphoreSlim>> _semaphoreSlims =
            new ConcurrentDictionary<string, RefCounted<SemaphoreSlim>>();

        public void Dispose()
        {
            lock (_semaphoreSlims)
            {
                foreach (var semaphoreSlimsValue in _semaphoreSlims.Values) semaphoreSlimsValue.Value.Dispose();

                _semaphoreSlims.Clear();
            }
        }

        public async Task<IDisposable> CreateLockAsync(string resource, TimeSpan? expiryTime = null,
            TimeSpan? waitTime = null,
            TimeSpan? retryTime = null, CancellationToken cancellationToken = new CancellationToken())
        {
            Exception innerException = null;
            try
            {
                var waitResult = await GetOrCreate(resource)
                    .WaitAsync(waitTime ?? TimeSpan.FromSeconds(30), cancellationToken);
                if (waitResult) return new SemaphoreReleaser(resource, Release);
            }
            catch (Exception e)
            {
                innerException = e;
            }

            throw new DistributedLockException(resource, null, DistributedLockBadStatus.Conflicted, innerException);
        }

        private SemaphoreSlim GetOrCreate(string key)
        {
            lock (_semaphoreSlims)
            {
                return _semaphoreSlims.AddOrUpdate(key,
                    _ => new RefCounted<SemaphoreSlim>(new SemaphoreSlim(1, 1)),
                    (_, value) =>
                    {
                        value.RefCount++;
                        return value;
                    }).Value;
            }
        }

        private void Release(string key)
        {
            RefCounted<SemaphoreSlim> item;
            lock (_semaphoreSlims)
            {
                item = _semaphoreSlims[key];
                item.RefCount--;
                if (item.RefCount == 0)
                    _semaphoreSlims.Remove(key, out _);
            }

            item.Value.Release();
        }

        private sealed class RefCounted<T>
        {
            public readonly T Value;

            public int RefCount;

            public RefCounted(T value)
            {
                RefCount = 1;
                Value = value;
            }
        }

        private class SemaphoreReleaser : IDisposable
        {
            private readonly string _key;
            private readonly Action<string> _releaseAction;

            public SemaphoreReleaser(string key, Action<string> releaseAction)
            {
                _key = key;
                _releaseAction = releaseAction;
            }

            public void Dispose()
            {
                _releaseAction(_key);
            }
        }
    }
}