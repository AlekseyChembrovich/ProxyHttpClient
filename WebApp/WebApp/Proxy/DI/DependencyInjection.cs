using WebApp.Proxy.DI;
using WebApp.Proxy.Factory;
using WebApp.Proxy.Provider;
using Microsoft.Extensions.Http;

namespace OkParser.Infrastructure.Client.Common.Proxy.DI;

/// <summary>
/// Dependency injection extension
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Method adds named http client for proxying
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="name">Name to create http client</param>
    /// <returns>Http client builder</returns>
    public static IHttpClientBuilder AddProxyHttpClient(this IServiceCollection services, string name)
    {
        services.AddHttpClient();

        var builder = new ProxyClientBuilder(services, name);

        return builder;
    }

    /// <summary>
    /// Method adds typed http client for proxying
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Http client builder</returns>
    public static IHttpClientBuilder AddProxyHttpClient<TClient, TImplementation>(this IServiceCollection services)
        where TClient : class
        where TImplementation : class, TClient
    {
        services.AddHttpClient();

        var clientName = typeof(TClient).Name;
        var implementationName = typeof(TImplementation).Name;
        var name = $"{clientName}_{implementationName}";

        var builder = new ProxyClientBuilder(services, name);
        
        services.AddTransient<TClient, TImplementation>(provider =>
        {
            var proxyClientFactory = provider.GetRequiredService<IProxyClientFactory>();
            var httpClient = proxyClientFactory.CreateClient(builder.Name);
            var typedClientFactory = provider.GetRequiredService<ITypedHttpClientFactory<TImplementation>>();

            return typedClientFactory.CreateClient(httpClient);
        });

        return builder;
    }

    /// <summary>
    /// Method configures proxy provider
    /// </summary>
    /// <param name="builder">Http client builder</param>
    /// <param name="configureProvider">Provider configuration</param>
    /// <returns>Http client builder</returns>
    public static IHttpClientBuilder ConfigureProxyProvider(
        this IHttpClientBuilder builder, Func<IServiceProvider, IProxyProvider> configureProvider)
    {
        builder.Services.Configure<ProxyFactoryOptions>(
            builder.Name, options => options.ConfigureProvider = configureProvider);

        return builder;
    }
}
