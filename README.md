![Build and Test](https://github.com/ch1seL/RedisLock/workflows/Build%20and%20Test/badge.svg)

This package is just [RedLock.net](https://github.com/samcook/RedLock.net) wrapper and simplifies its use within NetCore

# Usage

## Run redis server on you machine first

`docker run -d -p 6379:6379 redis`


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

# Sample

https://github.com/ch1seL/RedisLock/tree/master/samples/WorkerService
