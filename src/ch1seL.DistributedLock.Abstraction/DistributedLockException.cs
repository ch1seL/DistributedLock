using System;

namespace Microsoft.Extensions.Caching.Distributed
{
    public class DistributedLockException : Exception
    {
        public DistributedLockException(string resource, string lockId, DistributedLockBadStatus status, Exception innerException = null) : base(
            "Lock is not acquired", innerException)
        {
            base.Data.Add("Resource", resource);
            base.Data.Add("LockId", lockId);
            base.Data.Add("Status", status);
        }
    }
}