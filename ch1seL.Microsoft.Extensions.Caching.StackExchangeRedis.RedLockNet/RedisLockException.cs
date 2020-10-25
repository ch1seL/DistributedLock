using System;
using RedLockNet;

namespace Microsoft.Extensions.Caching
{
    public class RedisLockException : Exception {
        public RedisLockException(string resource, string lockId, RedLockStatus status) : base("Lock is not acquired") {
            base.Data.Add("Resource", resource);
            base.Data.Add("LockId", lockId);
            base.Data.Add("Status", status);
        }
    }
}