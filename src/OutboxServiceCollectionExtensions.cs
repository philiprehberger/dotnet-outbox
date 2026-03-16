using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Philiprehberger.Outbox;

/// <summary>
/// Extension methods for registering outbox services in the dependency injection container.
/// </summary>
public static class OutboxServiceCollectionExtensions
{
    /// <summary>
    /// Adds the outbox relay background service and configures outbox options.
    /// <para>
    /// You must also register an <see cref="IOutboxStore"/> and an <see cref="IOutboxDispatcher"/>
    /// implementation in the service collection.
    /// </para>
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">An optional action to configure <see cref="OutboxOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOutbox(
        this IServiceCollection services,
        Action<OutboxOptions>? configure = null)
    {
        var options = new OutboxOptions();
        configure?.Invoke(options);

        services.TryAddSingleton(options);
        services.AddHostedService<OutboxRelayService>();

        return services;
    }
}
