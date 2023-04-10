using System.Net;
using System.Net.Sockets;
using WebApp.Proxy.Provider;

namespace WebApp.Proxy.Handler;

/// <summary>
/// Message handler for handling proxying failure
/// </summary>
public class ProxyMessageHandler : HttpMessageHandler
{
    private readonly string _clientName;
    private readonly IProxyMessageHandlerFactory _factory;
    private readonly IProxyProvider _proxyProvider;
    private readonly ILogger<ProxyMessageHandler> _logger;

    /// <summary>
    /// Constructor of proxy message handler
    /// </summary>
    public ProxyMessageHandler(
        string clientName,
        IProxyMessageHandlerFactory factory,
        IProxyProvider proxyProvider,
        ILogger<ProxyMessageHandler> logger)
    {
        _clientName = clientName;
        _factory = factory;
        _proxyProvider = proxyProvider;
        _logger = logger;
    }

    /// <summary>
    /// The method sends a request to a third party API.
    /// If the response model is erroneous and contains a code confirming that the error is related to proxying or blocking by id,
    /// then a second attempt is made to send the request with new credentials.
    /// </summary>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage httpResponse = null;
        var needHandle = false;
        bool? existNext = null;

        do
        {
            if (needHandle)
            {
                existNext = _proxyProvider.MoveNext();
            }
            
            string proxyName = needHandle && _proxyProvider.Current is not null
                ? _proxyProvider.Current.Url
                : default;
            
            var wrapper = _factory.CreateHandler(_clientName, proxyName);
            if (needHandle && _proxyProvider.Current is not null)
            {
                var httpClientHandler = GetHttpClientHandler(wrapper);
                if (httpClientHandler is not null && httpClientHandler.Proxy is null)
                {
                    var address = new Uri(_proxyProvider.Current.Url);
                    httpClientHandler.Proxy = new WebProxy(address);
                }
            }

            try
            {
                httpResponse = await wrapper.CallSendAsync(request, cancellationToken);
                var content = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                needHandle = ToHandleResponse(content);
            }
            catch (HttpRequestException ex) when (needHandle)
            {
                var message = ex.InnerException switch
                {
                    SocketException { SocketErrorCode: SocketError.ConnectionRefused } =>
                        "Connection by proxy [{0}] was refused. Error message: {1}",
                    SocketException { SocketErrorCode: SocketError.TimedOut } =>
                        "Connection [{0}] attempt failed because party didn't properly respond after period of time. Error message: {1}",
                    _ => "Error occurred while interaction with server ({0}). Error message: {1}"
                };

                _logger.LogWarning(message, _proxyProvider.Current.Url, ex.Message);

                needHandle = true;
            }

            if (needHandle && existNext.HasValue && !existNext.Value)
            {
                throw new Exception("No more proxy servers available.");
            }
        }
        while (needHandle);

        return httpResponse;
    }

    /// <summary>
    /// Method gets http client handler from created http message handler
    /// </summary>
    /// <param name="handler">Http message handler</param>
    /// <returns>Http client handler</returns>
    private static HttpClientHandler GetHttpClientHandler(HttpMessageHandler handler)
    {
        var innerHandler = handler;

        while ((innerHandler is DelegatingHandler current) && (innerHandler is not HttpClientHandler))
        {
            innerHandler = current.InnerHandler;
        }

        return innerHandler as HttpClientHandler;
    }
    
    /// <summary>
    /// Method checks that response should be processed
    /// </summary>
    /// <param name="content">Content</param>
    /// <returns>Flag indicates that response should be processed</returns>
    private static bool ToHandleResponse(string content)
    {
        // TODO: Handle response context

        return false;
    }
}
