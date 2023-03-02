using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using StackExchange.Redis;
using Xunit;
using Xunit.Abstractions;

namespace ch1seL.DistributedLock.Tests.Fixtures;

public class RedisFixture : ITestFixture, IAsyncLifetime {
    private IContainer _redisContainer;

    public async Task InitializeAsync() {
        _redisContainer = new ContainerBuilder().WithImage("redis:7.0").WithPortBinding(6379, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("redis-cli", "ping")).Build();

        await _redisContainer.StartAsync();
    }

    public async Task DisposeAsync() {
        await _redisContainer.StopAsync();
    }

    public IServiceCollection Registration(IServiceCollection services, ITestOutputHelper output) {
        services.AddLogging(builder => builder.AddProvider(new SerilogLoggerProvider(new LoggerConfiguration()
            .MinimumLevel.Verbose().WriteTo.TestOutput(output).CreateLogger())));

        var configuration = $"localhost:{_redisContainer.GetMappedPublicPort(6379)}";
        services.AddStackExchangeRedisLock(options => options.Configuration = configuration);
        services.AddSingleton(_ => ConnectionMultiplexer.Connect(configuration));
        services.AddTransient(provider =>
            provider.GetRequiredService<ConnectionMultiplexer>().GetServer(configuration));
        services.AddTransient(provider => provider.GetRequiredService<ConnectionMultiplexer>().GetDatabase());
        return services;
    }
}

[CollectionDefinition("Redis collection")]
public class RedisCollection : ICollectionFixture<RedisFixture> { }