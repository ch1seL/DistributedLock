using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Caching.Distributed
{
    public class MemoryLock : IDistributedLock, IDisposable
    {
        public static readonly string SemaphoreWrapperTypeFullName = typeof(SemaphoreWrapper).FullName;
        private readonly object _lockWrapperMap = new object();
        private readonly Dictionary<string, SemaphoreWrapper> _wrapperMap = new Dictionary<string, SemaphoreWrapper>();

        public void Dispose()
        {
            lock (_lockWrapperMap)
            {
                _wrapperMap.Clear();
            }
        }

        public async Task<IDisposable> CreateLockAsync(string resource, TimeSpan? expiryTime = null, TimeSpan? waitTime = null, TimeSpan? retryTime = null,
            CancellationToken cancellationToken = default)
        {
            SemaphoreWrapper wrapper;
            lock (_lockWrapperMap)
            {
                if (!_wrapperMap.ContainsKey(resource))
                {
                    wrapper = new SemaphoreWrapper(resource, RemoveWrapper);
                    _wrapperMap.Add(resource, wrapper);
                }
                else
                {
                    wrapper = _wrapperMap[resource];
                }
            }

            return await wrapper.WaitAsync(waitTime ?? TimeSpan.FromSeconds(30), cancellationToken);
        }

        private void RemoveWrapper(string key)
        {
            lock (_lockWrapperMap)
            {
                _wrapperMap.Remove(key);
            }
        }

        private class SemaphoreWrapper : IDisposable
        {
            private readonly Action<string> _removeAction;
            private readonly string _resource;
            private readonly SemaphoreSlim _semaphoreSlim;
            private readonly object _userCountLock = new object();
            private int _useCount;

            public SemaphoreWrapper(string resource, Action<string> removeAction)
            {
                _resource = resource;
                _removeAction = removeAction;
                _semaphoreSlim = new SemaphoreSlim(1, 1);
            }

            public void Dispose()
            {
                _semaphoreSlim.Release();
                DecrementCount();
                RemoveIfNotUsed();
            }

            public async Task<IDisposable> WaitAsync(TimeSpan waitTime, CancellationToken cancellationToken)
            {
                IncrementCount();
                Exception innerException = null;

                try
                {
                    var waitResult = await _semaphoreSlim.WaitAsync(waitTime, cancellationToken);
                    if (waitResult) return this;
                }
                catch (Exception exception)
                {
                    innerException = exception;
                }

                DecrementCount();
                RemoveIfNotUsed();
                throw new DistributedLockException(_resource, null, DistributedLockBadStatus.Conflicted, innerException);
            }

            private void IncrementCount()
            {
                lock (_userCountLock)
                {
                    _useCount++;
                }
            }

            private void RemoveIfNotUsed()
            {
                if (_useCount != 0) return;
                _removeAction(_resource);
                InternalDispose();
            }

            private void DecrementCount()
            {
                lock (_userCountLock)
                {
                    _useCount--;
                }
            }

            private void InternalDispose()
            {
                _semaphoreSlim.Dispose();
            }
        }
    }
}