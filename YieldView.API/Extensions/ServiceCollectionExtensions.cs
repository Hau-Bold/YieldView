namespace YieldView.API.Extensions;

public static class ServiceCollectionExtensions
{
  /// <summary>
  /// Registers a BackgroundService that implements both IHostedService and a service interface.
  /// </summary>
  /// <typeparam name="TService">The concrete BackgroundService type.</typeparam>
  /// <typeparam name="TInterface">The interface implemented by the service.</typeparam>
  /// <param name="services">The IServiceCollection to add to.</param>
  /// <returns>The updated IServiceCollection.</returns>
  public static IServiceCollection AddBackgroundServiceWithInterface<TService, TInterface>(
      this IServiceCollection services)
      where TService : BackgroundService, TInterface
      where TInterface : class
  {
    services.AddHttpClient<TService>();
    services.AddHostedService<TService>();
    services.AddSingleton<TInterface>(sp =>
        sp.GetRequiredService<TService>());

    return services;
  }
}
