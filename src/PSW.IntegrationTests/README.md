# PSW Integration Tests

This project contains integration tests for the Package Script Writer API endpoints.

## Overview

The integration tests use xUnit and `Microsoft.AspNetCore.Mvc.Testing` to test the API endpoints in a realistic environment. The tests spin up a test server and make actual HTTP requests to verify the API behavior.

## Project Structure

- **CustomWebApplicationFactory.cs**: Custom factory class that sets up the test environment. This is where you can configure mock services or override dependencies for testing.
- **ScriptGeneratorApiTests.cs**: Integration tests for the ScriptGeneratorApi controller endpoints.
- **GlobalUsings.cs**: Common using statements for all test files.

## Running the Tests

### Using Visual Studio
1. Open the solution in Visual Studio
2. Open Test Explorer (Test → Test Explorer)
3. Click "Run All" to execute all tests

### Using .NET CLI
```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Using Visual Studio Code
1. Install the .NET Test Explorer extension
2. Tests will appear in the Test Explorer panel
3. Click the play button to run tests

## Writing New Tests

### Basic Test Structure

```csharp
public class MyApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public MyApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task MyEndpoint_ReturnsSuccess()
    {
        // Arrange
        // Set up test data

        // Act
        var response = await _client.GetAsync("/api/myendpoint");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
    }
}
```

### Testing POST Endpoints

```csharp
[Fact]
public async Task CreateItem_WithValidData_ReturnsCreated()
{
    // Arrange
    var request = new MyRequest
    {
        Name = "Test",
        Value = 123
    };

    // Act
    var response = await _client.PostAsJsonAsync("/api/items", request);

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    var result = await response.Content.ReadFromJsonAsync<MyResponse>();
    Assert.NotNull(result);
}
```

## Customizing the Test Environment

To customize services for testing, modify the `CustomWebApplicationFactory.ConfigureWebHost` method:

```csharp
protected override void ConfigureWebHost(IWebHostBuilder builder)
{
    builder.ConfigureServices(services =>
    {
        // Remove the real service
        var descriptor = services.SingleOrDefault(
            d => d.ServiceType == typeof(IMyService));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }

        // Add a mock service
        services.AddScoped<IMyService, MockMyService>();
    });

    builder.UseEnvironment("Testing");
}
```

## Test Coverage

The current test suite covers:
- ✅ GET /api/ScriptGeneratorApi/test - Basic connectivity test
- ✅ GET /api/ScriptGeneratorApi/clearcache - Cache clearing functionality
- ✅ POST /api/ScriptGeneratorApi/generatescript - Script generation with valid request
- ✅ POST /api/ScriptGeneratorApi/generatescript - Script generation with empty request
- ✅ POST /api/ScriptGeneratorApi/getpackageversions - Package version retrieval

## Best Practices

1. **Use IClassFixture**: Share the WebApplicationFactory across all tests in a class for better performance
2. **Test behavior, not implementation**: Focus on API contracts and expected responses
3. **Use realistic data**: Test with data that represents actual usage scenarios
4. **Clean up after tests**: If tests modify state, ensure proper cleanup
5. **Isolate tests**: Each test should be independent and not rely on other tests
6. **Use meaningful test names**: Name tests to describe what they're testing and expected outcome

## Dependencies

- xUnit 2.9.2
- Microsoft.AspNetCore.Mvc.Testing 10.0.0
- Microsoft.NET.Test.Sdk 17.12.0
- coverlet.collector 6.0.2 (for code coverage)
