# Fixing 42-Second API Delays: An IPv6 Timeout Mystery

**December 9, 2025** | Paul Seal

## The Problem That Drove Me Crazy

I was building a CLI tool for the Package Script Writer that calls various APIs to fetch Umbraco packages and generate installation scripts. Everything worked perfectly in the browser - API calls completed in under a second. But when I ran the same code from my console application, **every single API call took over 42 seconds**. Not 40 seconds, not 45 seconds, but consistently around 42 seconds.

This was maddening. The same API endpoints, the same network, but completely different performance characteristics. I was ready to throw my computer out the window.

## My First (Wrong) Assumption

Like many developers facing performance issues with async code, I immediately blamed `async/await`. I thought, "Maybe blocking synchronous calls would be faster?" So I converted all my async methods to synchronous ones using `.Result` and `.Wait()`.

**Spoiler alert:** This didn't help at all. The calls still took 42 seconds.

I want to apologize to async/await for doubting you. You weren't the problem.

## Building a Diagnostic Tool

Instead of blindly trying different solutions, I decided to be methodical. I created a diagnostic console application that would test different scenarios and measure exactly where the delay was happening.

Here's what I tested:

### Phase 1: Network Analysis
```
Test A: DNS Resolution
✓ DNS Resolution Time: 80ms
✓ IP Addresses found:
  - 2606:4700:3031::6815:2ab (InterNetworkV6)
  - 2606:4700:3035::ac43:817b (InterNetworkV6)
  - 172.67.129.123 (InterNetwork)
  - 104.21.2.171 (InterNetwork)
```

DNS was fast. Good. Let's continue.

```
Test B: IPv4 vs IPv6 Connectivity

IPv4 (172.67.129.123):
  ✓ TCP Connect Time: 11ms

IPv6 (2606:4700:3031::6815:2ab):
  ✗ TCP Connect failed after 21034ms
  ✗ Error: Connection timeout
```

**BINGO!** 🎯

## The Root Cause: IPv6 Timeout

My system was trying to connect via IPv6 first (as modern systems should). However, my IPv6 connectivity was broken somewhere in the network path. The system would:

1. Attempt IPv6 connection
2. Wait for timeout (~42 seconds)
3. Fall back to IPv4
4. Connect successfully in ~150ms

This explained everything:
- Why the first request took 42 seconds
- Why subsequent requests on the same connection were fast (37-113ms)
- Why browsers seemed faster (they might have been caching the IPv4 preference)

## The Test That Confirmed It

I created a test that forced IPv4-only connections:

```
Test 2: Force IPv4 Only
✓ Status: OK
✓ Time: 150ms  ← FROM 42 SECONDS TO 150ms!
✓ Response: Hello, world!
```

And for comparison, forcing IPv6:

```
Test 3: Force IPv6 Only
✗ Failed after 42045ms
✗ Error: Connection timeout
```

Perfect correlation. The problem was definitively IPv6 timeout.

## The Solution

The fix was to force IPv4-only connections in .NET using `SocketsHttpHandler` with a custom `ConnectCallback`:

```csharp
var handler = new SocketsHttpHandler
{
    ConnectCallback = async (context, cancellationToken) =>
    {
        // Force IPv4 to avoid ~42 second IPv6 timeout
        var socket = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream,
            ProtocolType.Tcp)
        {
            NoDelay = true
        };

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

var httpClient = new HttpClient(handler, disposeHandler: true)
{
    BaseAddress = new Uri(baseUrl),
    Timeout = TimeSpan.FromSeconds(90)
};
```

I applied this to:
- `ApiClient` for my custom API calls
- `IHttpClientFactory` configuration in DI for all services

## Restoring Async/Await

Once I fixed the IPv6 issue, I restored all the async/await code I had previously removed. Turns out async wasn't the problem - the 42-second delay was masking async's benefits.

With both fixes in place:
- **IPv4 fix** → Fast connections (~150ms instead of 42 seconds)
- **Async/await** → Non-blocking, responsive UI with beautiful Spectre.Console spinners

## Performance Results

### Before
- First API call: **42,413ms** (42 seconds!)
- Subsequent calls: 113ms, 37ms (fast because connection was reused)
- User experience: Application appears frozen

### After
- All API calls: **~150ms** (consistent performance)
- User experience: Fast and responsive with smooth UI spinners

## Lessons Learned

1. **Don't blame async/await for everything.** Performance issues can have many root causes. Measure first, optimize second.

2. **IPv6 timeout is a common issue.** If you're seeing exactly ~40-42 second delays, check IPv6 connectivity. This is a well-known problem in environments with broken IPv6.

3. **Build diagnostic tools.** Instead of guessing, create tools that measure and isolate the problem. My diagnostic tool pinpointed the issue in minutes.

4. **Connection reuse matters.** The fact that subsequent requests were fast should have been a clue that the initial connection was the problem, not the API itself.

5. **HTTP client best practices still apply.** Even though my HttpClient usage wasn't the root cause, I still improved it with:
   - Static instances instead of creating new ones
   - Proper connection pooling settings
   - IPv4-only configuration to avoid timeouts

## Alternative Solutions

If you face this issue, you have several options:

### Option 1: Force IPv4 in SocketsHttpHandler (My choice)
Most control, works per-HttpClient instance.

### Option 2: System-wide IPv4 preference
```json
{
  "configProperties": {
    "System.Net.PreferIPv4": true
  }
}
```

### Option 3: Environment variable
```bash
export DOTNET_SYSTEM_NET_SOCKETS_PREFER_IPV4=1
```

### Option 4: AppContext switch
```csharp
AppContext.SetSwitch("System.Net.DisableIPv6", true);
```

## The Diagnostic Tool

I've included the diagnostic tool in the repository at `src/PSW.ApiDiagnostic/`. If you're experiencing similar issues, you can run it to identify whether IPv6 timeout is your problem:

```bash
cd src/PSW.ApiDiagnostic
dotnet run
```

It will test DNS resolution, IPv4/IPv6 connectivity, and HTTP performance with various configurations.

## Conclusion

Performance debugging can be frustrating, but methodical investigation pays off. What seemed like a complex async/threading issue turned out to be a simple network configuration problem.

If your API calls are mysteriously slow:
1. Check if the delay is consistent (±1 second variation suggests timeout)
2. Look for ~40-42 second delays (classic IPv6 timeout)
3. Test IPv4 vs IPv6 connectivity
4. Force IPv4 if needed

And remember: Sometimes the answer isn't in your code, but in the network layer beneath it.

Happy debugging! 🐛

---

**Related Resources:**
- [HttpClient Best Practices - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines)
- [IPv6 Connectivity Issues in .NET](https://learn.microsoft.com/en-us/dotnet/core/compatibility/networking/6.0/socketshttphandler-uses-ipv6-first)
- [Package Script Writer on GitHub](https://github.com/prjseal/Package-Script-Writer)

**Tags:** #dotnet #performance #debugging #ipv6 #httpclient #async
