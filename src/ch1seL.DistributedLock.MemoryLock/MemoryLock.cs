using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Microsoft.Extensions.Caching
{
    public class MemoryLock : IDistributedLock, IDisposable
    {
        private readonly object _lock = new object();

        private readonly Dictionary<string, SemaphoreWrapper> _wrapperMap = new Dictionary<string, SemaphoreWrapper>();

        private bool _isDisposed;

        public void Dispose()
        {
            if (_isDisposed)
                return;

            lock (_lock)
            {
                foreach (var key in _wrapperMap.Keys)
                {
                    _wrapperMap.Remove(key, out SemaphoreWrapper wrapper);
                    wrapper.InternalDispose();
                }
            }

            _isDisposed = true;
        }

        public Task<IDisposable> CreateLockAsync(string resource, TimeSpan? expiryTime = null, TimeSpan? waitTime = null, TimeSpan? retryTime = null,
            CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (!_wrapperMap.ContainsKey(resource))
                    _wrapperMap.Add(resource, new SemaphoreWrapper(resource, w => Release(resource, w)));

                return _wrapperMap[resource].WaitAsync(waitTime ?? TimeSpan.FromMinutes(1), cancellationToken);
            }
        }

        private void Release(string key, SemaphoreWrapper wrapper)
        {
            lock (_lock)
            {
                var isEmpty = wrapper.Release();
                if (!isEmpty)
                    return;
                _wrapperMap.Remove(key);
                wrapper.InternalDispose();
            }
        }

        public class SemaphoreWrapper : IDisposable
        {
            private readonly Action<SemaphoreWrapper> _parentRelease;
            private readonly string _resource;
            private readonly SemaphoreSlim _semaphoreSlim;
            private readonly object _userCountLock = new object();
            private int _useCount;

            public SemaphoreWrapper(string resource, Action<SemaphoreWrapper> parentRelease)
            {
                _resource = resource;
                _parentRelease = parentRelease;
                _semaphoreSlim = new SemaphoreSlim(1, 1);
            }

            public void Dispose()
            {
                _parentRelease(this);
            }

            public async Task<IDisposable> WaitAsync(TimeSpan waitTime, CancellationToken cancellationToken)
            {
                lock (_userCountLock)
                {
                    _useCount++;
                }

                try
                {
                    var waitResult = await _semaphoreSlim.WaitAsync(waitTime, cancellationToken);
                    if (waitResult) return this;
                }
                catch (ObjectDisposedException)
                {
                }

                throw new DistributedLockException(_resource, null, DistributedLockBadStatus.Conflicted);
            }

            public bool Release()
            {
                _semaphoreSlim.Release();
                lock (_userCountLock)
                {
                    _useCount--;
                }

                return _useCount == 0;
            }

            public void InternalDispose()
            {
                _semaphoreSlim.Dispose();
            }
        }
    }
}