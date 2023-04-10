using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

namespace WebApp.Proxy.Handler;

/// <summary>
/// Proxy message handler factory
/// </summary>
public interface IProxyMessageHandlerFactory
{
    /// <summary>
    /// Method creates proxy message handler for proxying
    /// </summary>
    /// <param name="clientName">Client name</param>
    /// <param name="proxyName">Proxy name</param>
    /// <returns>Proxy message handler wrapper</returns>
    ProxyMessageHandlerWrapper CreateHandler(string clientName, string proxyName);
}

/// <summary>
/// Proxy message handler factory
/// </summary>
public class ProxyMessageHandlerFactory : IProxyMessageHandlerFactory
{
    private readonly IHttpMessageHandlerFactory _messageHandlerFactory;
    private readonly IOptionsMonitor<HttpClientFactoryOptions> _optionsMonitor;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Constructor of proxy message handler factory
    /// </summary>
    public ProxyMessageHandlerFactory(
        IHttpMessageHandlerFactory messageHandlerFactory,
        IOptionsMonitor<HttpClientFactoryOptions> optionsMonitor,
        IServiceProvider serviceProvider)
    {
        _messageHandlerFactory = messageHandlerFactory;
        _optionsMonitor = optionsMonitor;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Method creates proxy message handler for proxying
    /// </summary>
    /// <param name="clientName">Client name</param>
    /// <param name="proxyName">Proxy name</param>
    /// <returns>Proxy message handler wrapper</returns>
    public ProxyMessageHandlerWrapper CreateHandler(string clientName, string proxyName = null)
    {
        var name = string.IsNullOrWhiteSpace(proxyName)
            ? clientName
            : $"{clientName}_{proxyName}";
        
        var primary = _messageHandlerFactory.CreateHandler(name);

        if (string.IsNullOrWhiteSpace(proxyName))
        {
            return new ProxyMessageHandlerWrapper(primary);
        }

        var additional = GetAdditionalHandlers(clientName);
            
        var next = primary;
        for (var i = additional.Count - 1; i >= 0; i--)
        {
            var handler = additional[i];
            handler.InnerHandler = next;
            next = handler;
        }
            
        return new ProxyMessageHandlerWrapper(next);
    }

    /// <summary>
    /// Method gets list of additional handlers from http client factory options
    /// </summary>
    /// <param name="clientName">Client name</param>
    /// <returns>List of additional handlers</returns>
    private IList<DelegatingHandler> GetAdditionalHandlers(string clientName)
    {
        using var scope = _serviceProvider.CreateScope();
        HttpClientFactoryOptions options = _optionsMonitor.Get(clientName);

        var builder = scope.ServiceProvider.GetRequiredService<HttpMessageHandlerBuilder>();
        builder.Name = clientName;

        foreach (var messageHandlerBuilderAction in options.HttpMessageHandlerBuilderActions)
        {
            messageHandlerBuilderAction(builder);
        }
        
        return builder.AdditionalHandlers;
    }
}
