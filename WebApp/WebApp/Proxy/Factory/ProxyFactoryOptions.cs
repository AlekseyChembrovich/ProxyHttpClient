using WebApp.Proxy.Provider;

namespace WebApp.Proxy.Factory;

/// <summary>
/// Proxy factory options
/// </summary>
public class ProxyFactoryOptions
{
    /// <summary>
    /// Provider configuration
    /// </summary>
    public Func<IServiceProvider, IProxyProvider> ConfigureProvider;
}
