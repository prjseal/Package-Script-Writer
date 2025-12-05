using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace PSW.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory for integration testing
/// This class can be extended to customize the test environment (e.g., mock services, use in-memory database)
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Here you can override services for testing
            // For example, replace real services with mocks or use in-memory databases

            // Example: Replace a service with a mock
            // var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMyService));
            // if (descriptor != null)
            // {
            //     services.Remove(descriptor);
            // }
            // services.AddScoped<IMyService, MockMyService>();
        });

        builder.UseEnvironment("Testing");
    }
}
