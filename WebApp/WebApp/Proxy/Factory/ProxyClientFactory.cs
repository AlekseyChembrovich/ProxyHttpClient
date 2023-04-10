using WebApp.Proxy.Handler;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

namespace WebApp.Proxy.Factory;

/// <summary>
/// Proxy client factory
/// </summary>
public interface IProxyClientFactory : IHttpClientFactory
{
    
}

/// <summary>
/// Proxy client factory
/// </summary>
public class ProxyClientFactory : IProxyClientFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IProxyMessageHandlerFactory _proxyMessageHandlerFactory;
    private readonly IOptionsMonitor<HttpClientFactoryOptions> _optionsMonitor;
    private readonly IOptionsMonitor<ProxyFactoryOptions> _proxyOptionsMonitor;
    private readonly ILogger<ProxyMessageHandler> _logger;

    /// <summary>
    /// Constructor of proxy client factory
    /// </summary>
    public ProxyClientFactory(
        IServiceProvider serviceProvider,
        IProxyMessageHandlerFactory proxyMessageHandlerFactory,
        IOptionsMonitor<HttpClientFactoryOptions> optionsMonitor,
        IOptionsMonitor<ProxyFactoryOptions> proxyOptionsMonitor,
        ILogger<ProxyMessageHandler> logger)
    {
        _serviceProvider = serviceProvider;
        _proxyMessageHandlerFactory = proxyMessageHandlerFactory;
        _optionsMonitor = optionsMonitor;
        _proxyOptionsMonitor = proxyOptionsMonitor;
        _logger = logger;
    }

    /// <summary>
    /// Method creates and configures http client
    /// </summary>
    /// <param name="name">Name to create client</param>
    /// <returns>Http client</returns>
    public HttpClient CreateClient(string name)
    {
        var handler = CreateHandler(name);
        var client = new HttpClient(handler, disposeHandler: false);
        
        HttpClientFactoryOptions options = _optionsMonitor.Get(name);
        foreach (var httpClientAction in options.HttpClientActions)
        {
            httpClientAction(client);
        }

        return client;
    }

    /// <summary>
    /// Method creates message handler for proxying
    /// </summary>
    /// <param name="name">Name to create handler</param>
    /// <returns>Proxy message handler</returns>
    private HttpMessageHandler CreateHandler(string name)
    {
        ProxyFactoryOptions proxyFactoryOptions = _proxyOptionsMonitor.Get(name);

        var proxyProvider = proxyFactoryOptions.ConfigureProvider(_serviceProvider);
        
        var proxiedHandler = new ProxyMessageHandler(name, _proxyMessageHandlerFactory, proxyProvider, _logger);
        
        return proxiedHandler;
    }
}
