using Microsoft.Extensions.Caching.Distributed;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    ///     Extension methods for setting up Memory lock related services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class MemoryLockServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds Redis distributed lock services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
        public static IServiceCollection AddMemoryLock(this IServiceCollection services)
        {
            services.Add(ServiceDescriptor.Singleton<IDistributedLock, MemoryLock>());

            return services;
        }
    }
}