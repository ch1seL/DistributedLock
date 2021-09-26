using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Caching.Distributed
{
    public interface IDistributedLock
    {
        Task<IDisposable> CreateLockAsync(string resource, TimeSpan? expiryTime = null, TimeSpan? waitTime = null, TimeSpan? retryTime = null,
            CancellationToken cancellationToken = default);
    }
}