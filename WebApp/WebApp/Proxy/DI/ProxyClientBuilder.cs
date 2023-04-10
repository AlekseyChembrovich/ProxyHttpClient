using Microsoft.Extensions.DependencyInjection;

namespace WebApp.Proxy.DI;

/// <summary>
/// Proxy client builder
/// </summary>
public class ProxyClientBuilder : IHttpClientBuilder
{
    public IServiceCollection Services { get; }
    
    public string Name { get; }

    public ProxyClientBuilder(IServiceCollection services, string name)
    {
        Services = services;
        Name = name;
    }
}
