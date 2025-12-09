using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace PSW.ApiDiagnostic;

class Program
{
    private const string ApiUrl = "https://psw.codeshare.co.uk/api/scriptgeneratorapi/test";
    private const string Hostname = "psw.codeshare.co.uk";

    // Static HttpClient - best practice for reuse
    private static readonly HttpClient StaticClient = new();

    static async Task Main(string[] args)
    {
        Console.WriteLine("=== API Performance Diagnostic Tool (Enhanced) ===\n");
        Console.WriteLine($"Testing endpoint: {ApiUrl}\n");

        // New Test: DNS Resolution
        Console.WriteLine("=== PHASE 1: DNS & Network Analysis ===\n");

        Console.WriteLine("Test A: DNS Resolution");
        await TestDnsResolution();
        Console.WriteLine();

        // New Test: IPv4 vs IPv6
        Console.WriteLine("Test B: IPv4 vs IPv6 Connectivity");
        await TestIpVersions();
        Console.WriteLine();

        // New Test: Raw TCP Connection
        Console.WriteLine("Test C: Raw TCP Connection Speed (Port 443)");
        await TestRawTcpConnection();
        Console.WriteLine();

        Console.WriteLine("=== PHASE 2: HTTP Client Tests ===\n");

        // Test 1: Using a static/reused HttpClient (recommended)
        Console.WriteLine("Test 1: Static/Reused HttpClient");
        await TestWithStaticClient();
        Console.WriteLine();

        // Test 2: Force IPv4 only
        Console.WriteLine("Test 2: Force IPv4 Only");
        await TestWithIpv4Only();
        Console.WriteLine();

        // Test 3: Force IPv6 only (if available)
        Console.WriteLine("Test 3: Force IPv6 Only");
        await TestWithIpv6Only();
        Console.WriteLine();

        // Test 4: Multiple sequential requests with static client
        Console.WriteLine("Test 4: Three Sequential Requests (Static Client - Reuse Connection)");
        await TestMultipleRequests();
        Console.WriteLine();

        Console.WriteLine("=== Diagnostic Complete ===");

        Console.WriteLine("\n=== ANALYSIS ===");
        Console.WriteLine("If DNS resolution is fast but first HTTP request is slow:");
        Console.WriteLine("  → Likely TLS/SSL handshake or IPv6 fallback issue");
        Console.WriteLine("\nIf IPv6 test times out or takes ~40s:");
        Console.WriteLine("  → Your system is trying IPv6 first and timing out");
        Console.WriteLine("  → Solution: Force IPv4 or disable IPv6 fallback");
        Console.WriteLine("\nIf subsequent requests are fast:");
        Console.WriteLine("  → Connection reuse works - keep connections alive!");

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    static async Task TestDnsResolution()
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var addresses = await Dns.GetHostAddressesAsync(Hostname);
            sw.Stop();

            Console.WriteLine($"✓ DNS Resolution Time: {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"✓ IP Addresses found:");
            foreach (var addr in addresses)
            {
                Console.WriteLine($"  - {addr} ({addr.AddressFamily})");
            }
        }
        catch (Exception ex)
        {
            sw.Stop();
            Console.WriteLine($"✗ DNS Resolution failed after {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"✗ Error: {ex.Message}");
        }
    }

    static async Task TestIpVersions()
    {
        var addresses = await Dns.GetHostAddressesAsync(Hostname);

        var ipv4 = addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
        var ipv6 = addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetworkV6);

        if (ipv4 != null)
        {
            Console.WriteLine($"\nIPv4 ({ipv4}):");
            await TestTcpConnect(ipv4, 443);
        }
        else
        {
            Console.WriteLine("✗ No IPv4 address found");
        }

        if (ipv6 != null)
        {
            Console.WriteLine($"\nIPv6 ({ipv6}):");
            await TestTcpConnect(ipv6, 443);
        }
        else
        {
            Console.WriteLine("✗ No IPv6 address found");
        }
    }

    static async Task TestTcpConnect(IPAddress address, int port)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            using var client = new TcpClient(address.AddressFamily);
            await client.ConnectAsync(address, port);
            sw.Stop();
            Console.WriteLine($"  ✓ TCP Connect Time: {sw.ElapsedMilliseconds}ms");
            client.Close();
        }
        catch (Exception ex)
        {
            sw.Stop();
            Console.WriteLine($"  ✗ TCP Connect failed after {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"  ✗ Error: {ex.Message}");
        }
    }

    static async Task TestRawTcpConnection()
    {
        var sw = Stopwatch.StartNew();
        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(Hostname, 443);
            sw.Stop();
            Console.WriteLine($"✓ TCP Connection Time: {sw.ElapsedMilliseconds}ms");
            client.Close();
        }
        catch (Exception ex)
        {
            sw.Stop();
            Console.WriteLine($"✗ TCP Connection failed after {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"✗ Error: {ex.Message}");
        }
    }

    static async Task TestWithIpv4Only()
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var handler = new SocketsHttpHandler
            {
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
                }
            };

            using var client = new HttpClient(handler);
            var response = await client.GetAsync(ApiUrl);
            sw.Stop();

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"✓ Status: {response.StatusCode}");
            Console.WriteLine($"✓ Time: {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"✓ Response: {content}");
        }
        catch (Exception ex)
        {
            sw.Stop();
            Console.WriteLine($"✗ Failed after {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"✗ Error: {ex.Message}");
        }
    }

    static async Task TestWithIpv6Only()
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var handler = new SocketsHttpHandler
            {
                ConnectCallback = async (context, cancellationToken) =>
                {
                    var socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
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
                }
            };

            using var client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromSeconds(45); // Shorter timeout for IPv6 test
            var response = await client.GetAsync(ApiUrl);
            sw.Stop();

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"✓ Status: {response.StatusCode}");
            Console.WriteLine($"✓ Time: {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"✓ Response: {content}");
        }
        catch (Exception ex)
        {
            sw.Stop();
            Console.WriteLine($"✗ Failed after {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"✗ Error: {ex.Message}");
        }
    }

    static async Task TestWithStaticClient()
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var response = await StaticClient.GetAsync(ApiUrl);
            sw.Stop();

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"✓ Status: {response.StatusCode}");
            Console.WriteLine($"✓ Time: {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"✓ Response: {content}");
        }
        catch (Exception ex)
        {
            sw.Stop();
            Console.WriteLine($"✗ Failed after {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"✗ Error: {ex.Message}");
        }
    }

    static async Task TestWithNewClient()
    {
        var sw = Stopwatch.StartNew();
        try
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(ApiUrl);
            sw.Stop();

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"✓ Status: {response.StatusCode}");
            Console.WriteLine($"✓ Time: {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"✓ Response: {content}");
        }
        catch (Exception ex)
        {
            sw.Stop();
            Console.WriteLine($"✗ Failed after {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"✗ Error: {ex.Message}");
        }
    }

    static async Task TestWithKeepAlive()
    {
        var sw = Stopwatch.StartNew();
        try
        {
            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, ApiUrl);
            request.Headers.Connection.Clear();
            request.Headers.ConnectionClose = false;

            var response = await client.SendAsync(request);
            sw.Stop();

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"✓ Status: {response.StatusCode}");
            Console.WriteLine($"✓ Time: {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"✓ Response: {content}");
        }
        catch (Exception ex)
        {
            sw.Stop();
            Console.WriteLine($"✗ Failed after {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"✗ Error: {ex.Message}");
        }
    }

    static async Task TestWithSocketsHttpHandler()
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var handler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(2),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
                EnableMultipleHttp2Connections = true
            };

            using var client = new HttpClient(handler);
            var response = await client.GetAsync(ApiUrl);
            sw.Stop();

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"✓ Status: {response.StatusCode}");
            Console.WriteLine($"✓ Time: {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"✓ Response: {content}");
        }
        catch (Exception ex)
        {
            sw.Stop();
            Console.WriteLine($"✗ Failed after {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"✗ Error: {ex.Message}");
        }
    }

    static async Task TestMultipleRequests()
    {
        for (int i = 1; i <= 3; i++)
        {
            Console.Write($"Request {i}: ");
            var sw = Stopwatch.StartNew();
            try
            {
                var response = await StaticClient.GetAsync(ApiUrl);
                sw.Stop();

                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"✓ {response.StatusCode} in {sw.ElapsedMilliseconds}ms - {content}");
            }
            catch (Exception ex)
            {
                sw.Stop();
                Console.WriteLine($"✗ Failed after {sw.ElapsedMilliseconds}ms - {ex.Message}");
            }
        }
    }
}
