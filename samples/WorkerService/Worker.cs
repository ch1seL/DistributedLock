using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly IDistributedLock _distributedLock;
        private readonly Guid _instanceId = Guid.NewGuid();
        private readonly ILogger<Worker> _logger;
        private readonly Random _random = new Random();

        public Worker(ILogger<Worker> logger, IDistributedLock distributedLock)
        {
            _logger = logger;
            _distributedLock = distributedLock;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (await _distributedLock.CreateLockAsync("test-lock", waitTime: TimeSpan.FromMinutes(5),
                    retryTime: TimeSpan.FromMilliseconds(10), cancellationToken: stoppingToken))
                {
                    _logger.LogInformation("App:{appId} | Instance: {instanceId} | Worker running at: {time}",
                        Program.AppId, _instanceId,
                        DateTimeOffset.Now);
                    await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
                    _logger.LogInformation("App:{appId} | Instance: {instanceId} | Worker finished at: {time}",
                        Program.AppId, _instanceId,
                        DateTimeOffset.Now);
                }

                // RedLock uses retries and will use mostly only one instance of the worker
                await Task.Delay(_random.Next(100, 200), stoppingToken);
            }
        }
    }
}