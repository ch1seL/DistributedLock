using System;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    ///     Extension methods for setting up Redis distributed lock related services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class RedisLockServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds Redis distributed lock services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="setupAction">
        ///     An <see cref="Action{RedisLockOptions}" /> to configure the provided
        ///     <see cref="RedisLockOptions" />.
        /// </param>
        /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
        public static IServiceCollection AddStackExchangeRedisLock(this IServiceCollection services,
            Action<RedisLockOptions> setupAction)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            if (setupAction == null) throw new ArgumentNullException(nameof(setupAction));

            services.AddOptions();
            services.Configure(setupAction);
            services.Add(ServiceDescriptor.Singleton<IDistributedLock, RedisLock>());

            return services;
        }
    }
}