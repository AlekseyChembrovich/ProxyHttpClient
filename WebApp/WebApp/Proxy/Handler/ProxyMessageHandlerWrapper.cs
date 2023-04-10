namespace WebApp.Proxy.Handler;

/// <summary>
/// Wrapper to completely resend proxy request
/// </summary>
public class ProxyMessageHandlerWrapper : DelegatingHandler
{
    public ProxyMessageHandlerWrapper(HttpMessageHandler innerHandler) : base(innerHandler)
    {
        
    }

    public Task<HttpResponseMessage> CallSendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return base.SendAsync(request, cancellationToken);
    }
}
