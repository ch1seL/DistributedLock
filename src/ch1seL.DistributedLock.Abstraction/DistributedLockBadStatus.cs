namespace Microsoft.Extensions.Caching.Distributed
{
    /// <summary>
    ///     reused RedLockNet.RedLockStatus enum
    /// </summary>
    public enum DistributedLockBadStatus
    {
        Unlocked,
        NoQuorum,
        Conflicted,
        Expired
    }
}