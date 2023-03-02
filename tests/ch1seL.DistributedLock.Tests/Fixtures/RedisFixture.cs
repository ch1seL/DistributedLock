using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

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

    public IServiceCollection Registration(IServiceCollection services) {
        return services.AddStackExchangeRedisLock(options =>
            options.Configuration = $"localhost:{_redisContainer.GetMappedPublicPort(6379)}");
    }
}

[CollectionDefinition("Redis collection")]
public class RedisCollection : ICollectionFixture<RedisFixture> { }
