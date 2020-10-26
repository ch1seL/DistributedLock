using System;

namespace Microsoft.Extensions.Caching.Distributed
{
    public class DistributedLockException : Exception
    {
        public DistributedLockException(string resource, string lockId, DistributedLockBadStatus status) : base(
            "Lock is not acquired")
        {
            Resource = resource;
            LockId = lockId;
            Status = status;

            base.Data.Add("Resource", resource);
            base.Data.Add("LockId", lockId);
            base.Data.Add("Status", status);
        }

        private string Resource { get; }
        private string LockId { get; }
        private DistributedLockBadStatus Status { get; }
    }
}