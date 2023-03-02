using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace ch1seL.DistributedLock.Tests.Fixtures;

public interface ITestFixture {
    IServiceCollection Registration(IServiceCollection services, ITestOutputHelper output);
}