
# Proxy http client

Implementation of proxy substitution under the hood of http client.

## Features

- Typed `AddProxyHttpClient` for registering a typed client using DI extensions, which we can get by DI injection.
- Named `AddProxyHttpClient` for registering a named client using DI extensions, which we can get by `HttpClientFactory`.
- `ConfigureProxyProvider` is used to configure the provider for proxy manipulation.

## Base items

- `ProxyClientFactory` to get proxy http client.
- `ProxyMessageHandler` to sustitute proxy in `HttpClientHandler`.
- `ProxyMessageHandlerFactory` to get `ProxyMessageHandlerWrapper`, that includes organized handlers pipeline.
- `ProxyFactoryOptions` to pass specific configurations.