using System;
using System.Diagnostics;

namespace ch1seL.DistributedLock.Tests.Base;

[DebuggerDisplay("{nameof(Start)}: {Start}, {nameof(End)}: {End}")]
public readonly struct Interval {
    public bool NotEquals(Interval other) {
        return !Equals(other);
    }

    public override bool Equals(object obj) {
        return obj is Interval other && Id.Equals(other.Id);
    }

    public override int GetHashCode() {
        return Id.GetHashCode();
    }

    public Interval(long start, long end) {
        Start = start;
        End = end;
        Id = Guid.NewGuid();
    }

    private long Start { get; }
    private long End { get; }
    private Guid Id { get; }

    public bool Intersect(Interval interval) {
        return (Start >= interval.Start && Start <= interval.End) || (End >= interval.Start && End <= interval.End);
    }
}
