using System;
using System.Threading.Tasks;
using ch1seL.DistributedLock.Tests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Xunit;
using Xunit.Abstractions;

namespace ch1seL.DistributedLock.Tests.RedisLockTests;

[Collection("Redis collection")]
public class RedisTests : IAsyncLifetime {
    private const string Resource = "test_key";
    private const string ResourceKey = $"redlock:{Resource}";
    private readonly ITestFixture _fixture;
    private readonly ITestOutputHelper _output;
    private IDistributedLock _distributedLock;
    private IDatabase _redisDatabase;
    private ServiceProvider _serviceProvider;

    public RedisTests(ITestOutputHelper output, RedisFixture fixture) {
        _output = output;
        _fixture = fixture;
    }

    public Task InitializeAsync() {
        IServiceCollection services = new ServiceCollection();
        _fixture.Registration(services, _output);
        _serviceProvider = services.BuildServiceProvider();
        _distributedLock = _serviceProvider.GetRequiredService<IDistributedLock>();
        _redisDatabase = _serviceProvider.GetRequiredService<IDatabase>();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync() {
        if (_serviceProvider != null) await _serviceProvider.DisposeAsync();
    }

    [Fact]
    public async Task ResourceShouldBeRemovedIfNoExceptions() {
        var keyBefore = false;

        var act = async () => {
            using var @lock = await _distributedLock.CreateLockAsync(Resource, TimeSpan.FromMinutes(1),
                TimeSpan.FromSeconds(1));
            keyBefore = _redisDatabase.KeyExists(ResourceKey);
            throw new Exception("test");
        };
        var keyAfter = _redisDatabase.KeyExists(ResourceKey);

        await act.Should().ThrowAsync<Exception>().WithMessage("test");
        keyBefore.Should().BeTrue();
        keyAfter.Should().BeFalse();
    }

    [Fact]
    public async Task ResourceShouldBeRemovedIfThrowException() {
        var keyBefore = false;

        var act = async () => {
            using var @lock = await _distributedLock.CreateLockAsync(Resource, TimeSpan.FromMinutes(1),
                TimeSpan.FromSeconds(1));
            keyBefore = _redisDatabase.KeyExists(ResourceKey);
        };
        var keyAfter = _redisDatabase.KeyExists(ResourceKey);

        await act.Should().NotThrowAsync();
        keyBefore.Should().BeTrue();
        keyAfter.Should().BeFalse();
    }

    [Fact]
    public async Task ResourceShouldBeRemovedIfExpired() {
        var keyBefore = false;
        TimeSpan? keyBeforeTtl = null;

        var act = async () => {
            using var @lock =
                await _distributedLock.CreateLockAsync(Resource, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            await Task.Delay(TimeSpan.FromSeconds(1));
            keyBefore = _redisDatabase.KeyExists(ResourceKey);
            keyBeforeTtl = _redisDatabase.KeyTimeToLive(new RedisKey(ResourceKey));
        };
        var keyAfter = _redisDatabase.KeyExists(ResourceKey);

        await act.Should().NotThrowAsync();
        keyBefore.Should().BeTrue();
        keyBeforeTtl.Should().BeGreaterThan(TimeSpan.FromSeconds(0.99));
        keyAfter.Should().BeFalse();
    }
}