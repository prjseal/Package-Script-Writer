# PSW API Diagnostic Tool (Enhanced)

## Purpose

This diagnostic tool identifies why API calls are taking 40+ seconds when they should be < 1 second. Based on initial testing, we know:
- ✅ The API itself is fast (< 100ms when connection is established)
- ✅ Connection reuse works perfectly (subsequent requests: 37-113ms)
- ❌ **Initial connection establishment takes ~42 seconds**

## Running the Tool

```bash
cd src/PSW.ApiDiagnostic
dotnet run
```

## What It Tests

The enhanced tool runs tests in two phases:

### PHASE 1: Network & DNS Analysis

**Test A: DNS Resolution**
- Measures how long DNS lookup takes
- Lists all IP addresses (IPv4 and IPv6) for the hostname
- **Expected:** < 100ms
- **If slow:** DNS server issues

**Test B: IPv4 vs IPv6 Connectivity**
- Tests TCP connection to both IPv4 and IPv6 addresses separately
- **This is the critical test!**
- If IPv6 takes ~40 seconds or times out → **Found the problem!**
- If IPv4 is fast but IPv6 is slow → Your system is attempting IPv6 first and timing out

**Test C: Raw TCP Connection**
- Tests basic TCP connection on port 443 (HTTPS)
- Uses system default IP version preference
- **Expected:** < 500ms
- **If slow:** Network routing issues

### PHASE 2: HTTP Client Tests

**Test 1: Static/Reused HttpClient**
- Uses a static `HttpClient` instance (best practice)
- First request may be slow, but connection is kept for reuse

**Test 2: Force IPv4 Only**
- **THIS IS THE KEY TEST!**
- Forces HttpClient to use only IPv4
- If this is FAST (~100ms) but Test 1 was slow (~42s) → **IPv6 timeout is the issue**

**Test 3: Force IPv6 Only**
- Forces HttpClient to use only IPv6
- Will timeout/fail if IPv6 connectivity is broken
- Timeout set to 45s to avoid long waits

**Test 4: Three Sequential Requests**
- Makes 3 requests using the same HttpClient
- Demonstrates connection reuse
- **Expected:** First request ~42s (if unfixed), subsequent requests < 200ms

## Interpreting Results

### Scenario 1: IPv6 Timeout (Most Likely)
```
Test B: IPv4 vs IPv6
  IPv4: ✓ TCP Connect: 120ms
  IPv6: ✗ TCP Connect failed after 42000ms (timeout)

Test 2 (Force IPv4): ✓ 150ms
Test 3 (Force IPv6): ✗ Failed after 45000ms
```
**Diagnosis:** Your system tries IPv6 first, waits ~40s for timeout, then falls back to IPv4.

**Solution:** Force IPv4 in your application (see below).

### Scenario 2: TLS/SSL Issue
```
Test B: Both IPv4 and IPv6 connect fast (< 200ms)
Test 1: Takes 42 seconds
```
**Diagnosis:** TLS handshake is slow (certificate validation, cipher negotiation).

**Solution:** Check TLS configuration, update .NET runtime, check for antivirus/proxy interference.

### Scenario 3: Network/Proxy Issue
```
Test A: DNS fast
Test B: TCP connections fast
Test 1: Still slow
```
**Diagnosis:** HTTP(S) proxy, firewall, or network policy causing delays.

**Solution:** Check proxy settings, corporate network policies, firewall rules.

## The Fix: Force IPv4

Based on the diagnostic results, if IPv6 is the issue, update your `ApiClient` or use this pattern:

### Solution 1: Force IPv4 in SocketsHttpHandler

```csharp
private static readonly HttpClient _httpClient = new HttpClient(new SocketsHttpHandler
{
    ConnectCallback = async (context, cancellationToken) =>
    {
        // Force IPv4
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.NoDelay = true;

        try
        {
            await socket.ConnectAsync(context.DnsEndPoint, cancellationToken);
            return new NetworkStream(socket, ownsSocket: true);
        }
        catch
        {
            socket.Dispose();
            throw;
        }
    },
    PooledConnectionLifetime = TimeSpan.FromMinutes(5),
    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2)
});
```

### Solution 2: System-Wide IPv4 Preference

Add to your application startup or `runtimeconfig.json`:

```json
{
  "configProperties": {
    "System.Net.SocketsHttpHandler.UseSocketsHttpHandler": true,
    "System.Net.PreferIPv4": true
  }
}
```

Or set an environment variable:
```bash
export DOTNET_SYSTEM_NET_SOCKETS_PREFER_IPV4=1
```

### Solution 3: AppContext Switch (Runtime)

In your `Main` or startup code:

```csharp
AppContext.SetSwitch("System.Net.DisableIPv6", true);
```

## Original Issues in Your Code

While investigating, we also identified these issues in `ApiClient.cs`:

### Issue 1: Creating New HttpClient Instances (Line 26-30)
```csharp
// WRONG - Creates new HttpClient every time
var httpClient = new HttpClient
{
    BaseAddress = new Uri(baseUrl),
    Timeout = TimeSpan.FromSeconds(90)
};
```

**Problems:**
- Socket exhaustion
- Connection pool issues
- No connection reuse between ApiClient instances

**Fix:** Use static HttpClient or IHttpClientFactory

### Issue 2: No IPv4/IPv6 Control
The code doesn't handle IPv6 fallback timeout issues, which is causing the 42-second delay.

## Recommended Complete Fix for ApiClient

```csharp
public class ApiClient
{
    private static readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly ILogger? _logger;

    static ApiClient()
    {
        var handler = new SocketsHttpHandler
        {
            // Force IPv4 to avoid timeout
            ConnectCallback = async (context, cancellationToken) =>
            {
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.NoDelay = true;

                try
                {
                    await socket.ConnectAsync(context.DnsEndPoint, cancellationToken);
                    return new NetworkStream(socket, ownsSocket: true);
                }
                catch
                {
                    socket.Dispose();
                    throw;
                }
            },
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
            EnableMultipleHttp2Connections = true
        };

        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(90)
        };
    }

    public ApiClient(string baseUrl, ILogger? logger = null)
    {
        _baseUrl = baseUrl;
        _logger = logger;
        // No longer create HttpClient here!
    }

    // Update methods to use _httpClient with full URLs
    // since we can't set BaseAddress on static client
}
```

## Additional Resources

- [HttpClient Best Practices](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines)
- [IPv6 Connectivity Issues in .NET](https://learn.microsoft.com/en-us/dotnet/core/compatibility/networking/6.0/socketshttphandler-uses-ipv6-first)
- [SocketsHttpHandler Configuration](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.socketshttphandler)
