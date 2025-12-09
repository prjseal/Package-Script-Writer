# PSW API Diagnostic Tool

## Purpose

This diagnostic tool tests various HttpClient configurations to identify performance issues when calling the Package Script Writer API. It helps diagnose why API calls might be taking 30+ seconds instead of sub-second response times.

## Running the Tool

```bash
cd src/PSW.ApiDiagnostic
dotnet run
```

## What It Tests

The tool runs 5 different test scenarios:

### Test 1: Static/Reused HttpClient (✅ Best Practice)
- Uses a static `HttpClient` instance that's reused across requests
- This is the recommended approach for .NET applications
- Should show the best performance after the first request

### Test 2: New HttpClient Instance (❌ Anti-Pattern)
- Creates a fresh `HttpClient` for each request
- This is what your current code does in `ApiClient.cs:26-30`
- **This is likely causing your performance issues**
- Problems with this approach:
  - Exhausts available sockets
  - DNS resolution issues
  - Connection pool exhaustion
  - Each request has to establish a new TCP connection

### Test 3: HttpClient with Connection Keep-Alive
- Explicitly sets connection headers
- Tests if keep-alive improves performance

### Test 4: HttpClient with SocketsHttpHandler
- Uses modern `SocketsHttpHandler` with optimized settings
- Better connection pooling and DNS handling

### Test 5: Multiple Sequential Requests
- Makes 3 requests in a row with the static client
- Should show improved performance on subsequent requests due to connection reuse

## Expected Results

If the issue is HttpClient instantiation:
- Test 1 should be fast (< 1 second)
- Test 2 might be slow on first run, worse on subsequent runs
- Tests 3-5 should be relatively fast

## The Root Cause

Your `ApiClient` class creates a new `HttpClient` for every instance:

```csharp
// src/PackageCliTool/Services/ApiClient.cs:26-30
var httpClient = new HttpClient
{
    BaseAddress = new Uri(baseUrl),
    Timeout = TimeSpan.FromSeconds(90)
};
```

This is a well-known anti-pattern in .NET that causes:
1. **Socket exhaustion** - Each HttpClient maintains its own connection pool
2. **DNS caching issues** - DNS changes aren't respected when using fresh instances
3. **Performance degradation** - Each request must establish new TCP connections
4. **Connection pool exhaustion** - The OS can run out of available ports

## Recommended Fix

Use one of these approaches:

### Option 1: Static HttpClient (Simple)
```csharp
private static readonly HttpClient _httpClient = new HttpClient();
```

### Option 2: IHttpClientFactory (Recommended for DI)
```csharp
// In your DI configuration
services.AddHttpClient<ApiClient>();

// In ApiClient constructor
public ApiClient(HttpClient httpClient, ILogger logger)
{
    _httpClient = httpClient;
    _logger = logger;
}
```

### Option 3: SocketsHttpHandler (Manual control)
```csharp
private static readonly HttpClient _httpClient = new HttpClient(new SocketsHttpHandler
{
    PooledConnectionLifetime = TimeSpan.FromMinutes(2)
});
```

## Additional Resources

- [HttpClient Best Practices](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines)
- [You're using HttpClient wrong](https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/)
