namespace WebApp.Proxy.Provider;

/// <summary>
/// Proxy provider
/// </summary>
public interface IProxyProvider
{
    /// <summary>
    /// Current proxy entry
    /// </summary>
    ProxyEntry Current { get; }

    /// <summary>
    /// Methods moves pointer to next proxy entry
    /// </summary>
    /// <returns>Flag indicates that pointer has been moved</returns>
    bool MoveNext();
}

/// <summary>
/// Proxy provider
/// </summary>
public class ProxyProvider : IProxyProvider
{
    private static readonly List<ProxyEntry> _proxyEntries = new()
    {
        new ProxyEntry
        {
            Url = "http://localhost:3130",
            IsBlocked = false
        },
        new ProxyEntry
        {
            Url = "http://localhost:3128",
            IsBlocked = false
        }
    };

    private readonly ILogger<ProxyProvider> _logger;

    /// <summary>
    /// Current proxy entry
    /// </summary>
    public ProxyEntry Current { get; private set; }

    /// <summary>
    /// Constructor of proxy provider
    /// </summary>
    public ProxyProvider(ILogger<ProxyProvider> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Methods moves pointer to next proxy entry
    /// </summary>
    /// <returns>Flag indicates that pointer has been moved</returns>
    public bool MoveNext()
    {
        if (!_proxyEntries.Any(x => !x.IsBlocked))
        {
            Current = null;
            _logger.LogWarning("No more proxy servers available.");

            return false;
        }

        if (Current is not null)
        {
            Current.IsBlocked = false;
        }

        var proxy = _proxyEntries.First(x => !x.IsBlocked);
        Current = proxy;

        _logger.LogInformation("Current proxy server [url:{1}] is set.", proxy.Url);
        
        return true;
    }
}
