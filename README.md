![Build and Test](https://github.com/ch1seL/RedisLock/workflows/Build%20and%20Test/badge.svg)

This package is just [RedLock.net](https://github.com/samcook/RedLock.net) wrapper and simplifies it using within NetCore

# Usage

## Run redis server on you machine first

`docker run -d -p 6379:6379 redis`

## Add ch1seL.DistributedLock.RedisLock to your project

`dotnet add package ch1seL.DistributedLock.RedisLock`

## Register in DI

```
Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddStackExchangeRedisLock(options => options.Configuration = "localhost");
    }
```

## Inject and lock some piece of code

```
public class Worker : BackgroundService
{
    private readonly IDistributedLock _distributedLock;

    public Worker(IDistributedLock distributedLock)
    {
        _distributedLock = distributedLock;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (await _distributedLock.CreateLockAsync("test-lock", cancellationToken: stoppingToken)) {
            await DoSomeStuff();
        }
    }
}
```

## Testing and local usage

For testing or local usage you can register in-memory implementation 
 
`dotnet add package ch1seL.DistributedLock.MemoryLock`
 
```
Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) => {
        if (hostContext.HostingEnvironment.IsDevelopment()) {
            services.AddMemoryLock();
        } else {
            services.AddStackExchangeRedisLock(options => options.Configuration = "localhost");    
        }
    }
```
# Sample

https://github.com/ch1seL/RedisLock/tree/master/samples/WorkerService
