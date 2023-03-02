using Microsoft.Extensions.DependencyInjection;

namespace ch1seL.DistributedLock.Tests.Fixtures;

public interface ITestFixture {
    IServiceCollection Registration(IServiceCollection services);
}
