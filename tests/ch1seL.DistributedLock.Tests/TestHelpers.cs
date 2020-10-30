using System;
using System.Linq;

namespace ch1seL.DistributedLock.Tests
{
    internal static class TestHelpers
    {
        public static string[] GenerateGuidKeys(int repeat)
        {
            return Enumerable.Repeat((Func<string>) (() => Guid.NewGuid().ToString("N")), repeat).Select(g => g()).ToArray();
        }
    }
}