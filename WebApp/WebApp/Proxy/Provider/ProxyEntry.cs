namespace WebApp.Proxy.Provider;

/// <summary>
/// Proxy entry
/// </summary>
public sealed class ProxyEntry
{
    /// <summary>
    /// Proxy address
    /// </summary>
    public string Url { get; init; }

    /// <summary>
    /// Is blocked
    /// </summary>
    public bool IsBlocked { get; set; }
}
