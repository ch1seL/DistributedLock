using System;

namespace Microsoft.Extensions.Caching.Distributed
{
    public class DistributedLockException : Exception
    {
        public DistributedLockException(string resource, string lockId, string status) : base("Lock is not acquired")
        {
            base.Data.Add("Resource", resource);
            base.Data.Add("LockId", lockId);
            base.Data.Add("Status", status);
        }
    }
}