using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ch1seL.DistributedLock.Tests.Fixtures;

public class InMemoryFixture : ITestFixture {
    public IServiceCollection Registration(IServiceCollection services) {
        return services.AddMemoryLock();
    }
}

[CollectionDefinition("InMemory collection")]
public class InMemoryCollection : ICollectionFixture<InMemoryFixture> { }
