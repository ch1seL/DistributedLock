using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace ch1seL.DistributedLock.Tests.Fixtures;

public class InMemoryFixture : ITestFixture {
    public IServiceCollection Registration(IServiceCollection services, ITestOutputHelper output) {
        services.AddLogging(builder => builder.AddProvider(new SerilogLoggerProvider(new LoggerConfiguration()
            .MinimumLevel.Verbose().WriteTo.TestOutput(output).CreateLogger())));

        return services.AddMemoryLock();
    }
}

[CollectionDefinition("InMemory collection")]
public class InMemoryCollection : ICollectionFixture<InMemoryFixture> { }