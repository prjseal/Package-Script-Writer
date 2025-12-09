using System.Diagnostics;
using System.Net;

namespace PSW.ApiDiagnostic;

class Program
{
    private const string ApiUrl = "https://psw.codeshare.co.uk/api/scriptgeneratorapi/test";

    // Static HttpClient - best practice for reuse
    private static readonly HttpClient StaticClient = new();

    static async Task Main(string[] args)
    {
        Console.WriteLine("=== API Performance Diagnostic Tool ===\n");
        Console.WriteLine($"Testing endpoint: {ApiUrl}\n");

        // Test 1: Using a static/reused HttpClient (recommended)
        Console.WriteLine("Test 1: Static/Reused HttpClient");
        await TestWithStaticClient();
        Console.WriteLine();

        // Test 2: Creating a new HttpClient each time (anti-pattern)
        Console.WriteLine("Test 2: New HttpClient Instance (Anti-Pattern)");
        await TestWithNewClient();
        Console.WriteLine();

        // Test 3: Using HttpClient with explicit connection settings
        Console.WriteLine("Test 3: HttpClient with Connection Keep-Alive");
        await TestWithKeepAlive();
        Console.WriteLine();

        // Test 4: Using HttpClient with DNS refresh disabled
        Console.WriteLine("Test 4: HttpClient with SocketsHttpHandler (No DNS Caching Issues)");
        await TestWithSocketsHttpHandler();
        Console.WriteLine();

        // Test 5: Multiple sequential requests with static client
        Console.WriteLine("Test 5: Three Sequential Requests (Static Client)");
        await TestMultipleRequests();
        Console.WriteLine();

        Console.WriteLine("=== Diagnostic Complete ===");
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
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
