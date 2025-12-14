# Testing Guide

Complete guide for testing the Package Script Writer application, including integration tests, API testing, and continuous integration.

## Table of Contents
- [Overview](#overview)
- [Integration Tests](#integration-tests)
- [API Testing](#api-testing)
- [Continuous Integration](#continuous-integration)
- [Writing Tests](#writing-tests)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)

---

## Overview

The Package Script Writer project uses a comprehensive testing strategy to ensure reliability and quality:

### Testing Stack

| Technology | Purpose | Version |
|------------|---------|---------|
| **xUnit** | Test framework | 2.9.2 |
| **Microsoft.AspNetCore.Mvc.Testing** | Integration testing | 10.0.0 |
| **FluentAssertions** | Assertion library | 6.12.2 |
| **HttpClient** | HTTP request testing | Built-in |
| **GitHub Actions** | CI/CD automation | - |

### Test Coverage

- ✅ API endpoint integration tests
- ✅ Script generation with various configurations
- ✅ Package version retrieval
- ✅ Cache management
- ✅ Error handling and validation
- ✅ HTTP status codes and responses

---

## Integration Tests

### Overview

Integration tests validate the API endpoints in a realistic environment by spinning up a test server using `WebApplicationFactory` and making actual HTTP requests. This approach tests the complete request/response pipeline.

### Test Project Structure

```
src/PSW.IntegrationTests/
├── PSW.IntegrationTests.csproj     # Project file with dependencies
├── ScriptGeneratorApiTests.cs      # Main test class
├── CustomWebApplicationFactory.cs  # Test server configuration
├── GlobalUsings.cs                 # Common using statements
└── README.md                       # Project documentation
```

### Running Integration Tests

**Command Line** (recommended):
```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test
dotnet test --filter "FullyQualifiedName~GenerateScript_WithValidRequest_ReturnsScript"

# Run tests in a specific class
dotnet test --filter "FullyQualifiedName~ScriptGeneratorApiTests"
```

**Visual Studio**:
1. Open Test Explorer: `Test` → `Test Explorer` (Ctrl+E, T)
2. Click "Run All" or select specific tests
3. View results in Test Explorer window

**Visual Studio Code**:
1. Install: [.NET Test Explorer](https://marketplace.visualstudio.com/items?itemName=formulahendry.dotnet-test-explorer)
2. Open Test Explorer panel
3. Click play button to run tests

**JetBrains Rider**:
1. Open Unit Tests window: `View` → `Tool Windows` → `Unit Tests`
2. Click "Run All" or select specific tests
3. View results with detailed output

---

### Current Test Coverage

The `ScriptGeneratorApiTests` class covers all major API endpoints:

#### 1. Health Check Endpoint
```csharp
[Fact]
public async Task Test_ReturnsSuccessStatusCode()
```
- **Tests**: GET /api/ScriptGeneratorApi/test
- **Validates**: API is running and accessible

#### 2. Cache Management
```csharp
[Fact]
public async Task ClearCache_ReturnsSuccessStatusCode()
```
- **Tests**: GET /api/ScriptGeneratorApi/clearcache
- **Validates**: Cache clearing functionality

#### 3. Script Generation - Valid Request
```csharp
[Fact]
public async Task GenerateScript_WithValidRequest_ReturnsScript()
```
- **Tests**: POST /api/ScriptGeneratorApi/generatescript
- **Validates**: Script generation with valid configuration
- **Checks**: Response contains "dotnet new install" command

#### 4. Script Generation - Empty Request
```csharp
[Fact]
public async Task GenerateScript_WithEmptyRequest_ReturnsScript()
```
- **Tests**: POST /api/ScriptGeneratorApi/generatescript
- **Validates**: Handling of empty/minimal requests
- **Checks**: Default script generation

#### 5. Package Version Retrieval
```csharp
[Fact]
public async Task GetPackageVersions_WithValidPackageId_ReturnsVersions()
```
- **Tests**: POST /api/ScriptGeneratorApi/getpackageversions
- **Validates**: NuGet package version lookup
- **Checks**: Returns version list from NuGet.org

---

### Test Architecture

#### CustomWebApplicationFactory

The `CustomWebApplicationFactory` class configures the test environment:

```csharp
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Configure test-specific services
            // Override dependencies if needed
        });

        builder.UseEnvironment("Testing");
    }
}
```

**Key Features**:
- Uses `WebApplicationFactory<Program>` for in-memory test server
- Inherits all application configuration
- Allows service replacement for mocking
- Sets "Testing" environment for test-specific configuration

#### Test Class Structure

```csharp
public class ScriptGeneratorApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ScriptGeneratorApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task TestName_Scenario_ExpectedResult()
    {
        // Arrange - Set up test data
        var request = new { /* test data */ };

        // Act - Execute the test
        var response = await _client.PostAsJsonAsync("/api/endpoint", request);

        // Assert - Verify results
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

**Benefits of IClassFixture**:
- Shares `WebApplicationFactory` across all tests in the class
- Improves performance by reusing test server
- Ensures proper setup and teardown

---

## API Testing

### Swagger UI (Recommended)

**Swagger UI** provides the easiest way to test the API interactively with full OpenAPI documentation.

#### Accessing Swagger UI

1. **Start the application**:
   ```bash
   dotnet watch run --project ./src/PSW/
   ```

2. **Open in browser**:
   - Local: [https://localhost:5001/api/docs](https://localhost:5001/api/docs)
   - Production: [https://psw.codeshare.co.uk/api/docs](https://psw.codeshare.co.uk/api/docs)

#### Features

- 📖 **Interactive Documentation**: Complete API reference with OpenAPI annotations
- 🧪 **Try It Out**: Execute requests directly from the browser
- 📝 **Examples**: Request/response samples for all endpoints
- 🔍 **Schemas**: Detailed model structure and validation rules
- 📄 **Spec Download**: Export OpenAPI JSON specification

#### Using Swagger UI

1. **Expand an endpoint** by clicking on it
2. Click **"Try it out"** button
3. **Fill in parameters** or request body
4. Click **"Execute"**
5. **View response** with status code, headers, and body

**Example: Generate Script**

```json
{
  "model": {
    "templateName": "Umbraco.Templates",
    "templateVersion": "14.3.0",
    "projectName": "TestProject",
    "useUnattendedInstall": true,
    "databaseType": "SQLite",
    "userFriendlyName": "Admin",
    "userEmail": "admin@example.com",
    "userPassword": "Password123"
  }
}
```

---

### REST Client (VS Code)

The repository includes `Api Request/API Testing.http` for testing with the [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) extension.

#### Setup

1. **Install extension**: REST Client by Huachao Mao
2. **Start application**: `dotnet watch run --project ./src/PSW/`
3. **Open file**: `Api Request/API Testing.http`
4. **Click** "Send Request" above any request

#### Example Requests

```http
### Test Health Check
GET https://localhost:5001/api/ScriptGeneratorApi/test

### Generate Script
POST https://localhost:5001/api/scriptgeneratorapi/generatescript
Content-Type: application/json

{
  "model": {
    "templateName": "Umbraco.Templates",
    "templateVersion": "14.3.0",
    "projectName": "TestProject"
  }
}

### Get Package Versions
POST https://localhost:5001/api/scriptgeneratorapi/getpackageversions
Content-Type: application/json

{
  "packageId": "Umbraco.Community.BlockPreview",
  "includePrerelease": false
}

### Clear Cache
GET https://localhost:5001/api/scriptgeneratorapi/clearcache
```

---

### cURL

Test API endpoints from the command line:

**Health Check**:
```bash
curl -k https://localhost:5001/api/ScriptGeneratorApi/test
```

**Generate Script**:
```bash
curl -X POST https://localhost:5001/api/scriptgeneratorapi/generatescript \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "model": {
      "templateName": "Umbraco.Templates",
      "templateVersion": "14.3.0",
      "projectName": "TestProject"
    }
  }'
```

**Get Package Versions**:
```bash
curl -X POST https://localhost:5001/api/scriptgeneratorapi/getpackageversions \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "packageId": "Umbraco.Community.BlockPreview",
    "includePrerelease": false
  }'
```

**Note**: The `-k` flag ignores SSL certificate validation (development only).

---

### PowerShell

**Test endpoint using PowerShell**:
```powershell
$body = @{
    model = @{
        templateName = "Umbraco.Templates"
        templateVersion = "14.3.0"
        projectName = "TestProject"
    }
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:5001/api/scriptgeneratorapi/generatescript" `
    -Method Post `
    -ContentType "application/json" `
    -Body $body `
    -SkipCertificateCheck
```

---

## Continuous Integration

### GitHub Actions Workflow

Every pull request automatically runs all tests via GitHub Actions, ensuring code quality and preventing regressions.

**Workflow File**: `.github/workflows/website-build-and-test.yml`

#### Workflow Configuration

```yaml
name: Integration Tests

on:
  pull_request:
    branches: [ main ]
  workflow_dispatch:

jobs:
  integration-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - name: Restore dependencies
        run: dotnet restore ./src/PSW.sln
      - name: Build solution
        run: dotnet build ./src/PSW.sln --configuration Release --no-restore
      - name: Run integration tests
        run: dotnet test ./src/PSW.IntegrationTests/PSW.IntegrationTests.csproj --no-build --verbosity normal
```

#### Features

- 🔄 **Automatic execution** on every pull request
- ✅ **Build verification** ensures solution compiles
- 🧪 **Test execution** runs all integration tests
- 📊 **Result reporting** shows pass/fail status
- 🚫 **PR protection** blocks merge if tests fail
- ⚡ **Fast feedback** results in ~2-3 minutes

#### Triggering the Workflow

**Automatically triggered**:
- On pull request creation/update to `main` branch
- Manual trigger via GitHub Actions UI (workflow_dispatch)

**Manual trigger** (via GitHub CLI):
```bash
gh workflow run website-build-and-test.yml
```

#### Viewing Results

1. Go to the **Pull Request** on GitHub
2. Click the **"Checks"** tab
3. View **"PR - Website - Build and Test"** workflow
4. Expand to see detailed test results

**From command line**:
```bash
# List workflow runs
gh run list --workflow=website-build-and-test.yml

# View specific run
gh run view <run-id>
```

---

## Writing Tests

### Creating a New Test

**1. Add test method to test class**:
```csharp
[Fact]
public async Task NewEndpoint_WithValidData_ReturnsSuccess()
{
    // Arrange
    var request = new
    {
        property1 = "value1",
        property2 = 123
    };

    // Act
    var response = await _client.PostAsJsonAsync(
        "/api/ScriptGeneratorApi/newendpoint", request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var content = await response.Content.ReadAsStringAsync();
    content.Should().NotBeNullOrEmpty();
}
```

### Using FluentAssertions

FluentAssertions provides readable, expressive assertions:

**Instead of**:
```csharp
Assert.Equal(HttpStatusCode.OK, response.StatusCode);
Assert.NotNull(result);
Assert.True(result.Contains("expected"));
```

**Use**:
```csharp
response.StatusCode.Should().Be(HttpStatusCode.OK);
result.Should().NotBeNull();
result.Should().Contain("expected");
```

**Common assertions**:
```csharp
// Status codes
response.StatusCode.Should().Be(HttpStatusCode.OK);
response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

// String content
content.Should().NotBeNullOrEmpty();
content.Should().Contain("expected text");
content.Should().StartWith("prefix");

// JSON properties
result.GetProperty("script").GetString().Should().NotBeNullOrEmpty();
result.GetProperty("versions").GetArrayLength().Should().BeGreaterThan(0);

// Collections
versions.Should().NotBeEmpty();
versions.Should().HaveCount(5);
versions.Should().Contain("1.0.0");
```

---

### Test Naming Convention

Use the pattern: `MethodName_Scenario_ExpectedResult`

**Good examples**:
- `GenerateScript_WithValidRequest_ReturnsScript`
- `GenerateScript_WithEmptyRequest_ReturnsDefaultScript`
- `GetPackageVersions_WithInvalidPackage_ReturnsNotFound`
- `ClearCache_Always_ReturnsSuccess`

**Poor examples**:
- `TestGenerateScript` (not descriptive)
- `Test1` (meaningless)
- `ScriptTest` (unclear)

---

### Testing Different Scenarios

**Test success case**:
```csharp
[Fact]
public async Task Endpoint_WithValidData_ReturnsSuccess()
{
    var response = await _client.PostAsJsonAsync("/api/endpoint", validData);
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

**Test error handling**:
```csharp
[Fact]
public async Task Endpoint_WithInvalidData_ReturnsBadRequest()
{
    var response = await _client.PostAsJsonAsync("/api/endpoint", invalidData);
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
}
```

**Test with different inputs** (Theory):
```csharp
[Theory]
[InlineData("Umbraco.Templates", "14.3.0")]
[InlineData("Umbraco.Templates", "13.5.0")]
[InlineData("Umbraco.Templates", "LTS")]
public async Task GenerateScript_WithDifferentVersions_ReturnsScript(
    string templateName, string version)
{
    var request = new { model = new { templateName, templateVersion = version } };
    var response = await _client.PostAsJsonAsync("/api/endpoint", request);
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

---

## Best Practices

### 1. Test Independence

Each test should be completely independent and not rely on other tests:

**✅ Good**:
```csharp
[Fact]
public async Task Test1_Scenario1_Result1()
{
    var data = CreateTestData();
    var response = await _client.PostAsJsonAsync("/api/endpoint", data);
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}

[Fact]
public async Task Test2_Scenario2_Result2()
{
    var data = CreateTestData();  // Create fresh data
    var response = await _client.PostAsJsonAsync("/api/endpoint", data);
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

**❌ Bad**:
```csharp
private static string sharedResult;

[Fact]
public async Task Test1_CreatesData()
{
    sharedResult = await CreateData();  // Don't share state
}

[Fact]
public async Task Test2_UsesSharedData()
{
    await _client.PostAsync("/api/endpoint", sharedResult);  // Depends on Test1
}
```

---

### 2. Arrange-Act-Assert Pattern

Structure tests clearly with the AAA pattern:

```csharp
[Fact]
public async Task MyTest()
{
    // Arrange - Set up test data and preconditions
    var request = new
    {
        model = new
        {
            templateName = "Umbraco.Templates",
            projectName = "TestProject"
        }
    };

    // Act - Execute the operation being tested
    var response = await _client.PostAsJsonAsync("/api/endpoint", request);

    // Assert - Verify the results
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var content = await response.Content.ReadAsStringAsync();
    content.Should().NotBeNullOrEmpty();
}
```

---

### 3. Test Realistic Scenarios

Use data that represents actual usage:

**✅ Good** (realistic):
```csharp
var request = new
{
    model = new
    {
        templateName = "Umbraco.Templates",
        templateVersion = "14.3.0",
        projectName = "MyBlogSite",
        useUnattendedInstall = true,
        databaseType = "SQLite",
        packages = "Umbraco.Community.BlockPreview|1.6.0"
    }
};
```

**❌ Bad** (unrealistic):
```csharp
var request = new
{
    model = new
    {
        templateName = "X",
        projectName = "A"
    }
};
```

---

### 4. Use IClassFixture

Share the test server across all tests for better performance:

```csharp
public class MyApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public MyApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // Tests here...
}
```

---

### 5. Test Both Success and Failure

Always test both happy path and error cases:

```csharp
// Success case
[Fact]
public async Task Endpoint_WithValidData_ReturnsSuccess() { }

// Error cases
[Fact]
public async Task Endpoint_WithInvalidData_ReturnsBadRequest() { }

[Fact]
public async Task Endpoint_WithMissingRequired_ReturnsBadRequest() { }

[Fact]
public async Task Endpoint_WithServerError_ReturnsInternalServerError() { }
```

---

### 6. Keep Tests Fast

Integration tests should be fast to encourage frequent running:

- ✅ Use in-memory test server (already done via `WebApplicationFactory`)
- ✅ Minimize external dependencies
- ✅ Use `IClassFixture` to share setup
- ❌ Avoid `Thread.Sleep()` or artificial delays
- ❌ Don't make unnecessary HTTP requests

---

### 7. Run Tests Frequently

Integrate testing into your development workflow:

```bash
# Run tests after making changes
dotnet test

# Use watch mode during development
dotnet watch test

# Run before committing
git add . && dotnet test && git commit -m "message"
```

---

## Troubleshooting

### Common Issues

#### Test Discovery Fails

**Symptom**: Tests don't appear in Test Explorer

**Solution**:
```bash
# Clean and rebuild
dotnet clean
dotnet build

# Restore packages
dotnet restore
```

---

#### Tests Fail with "Port Already in Use"

**Symptom**: Error message about port 5001 or 5000

**Solution**: The test server automatically uses random ports, but if you still see this:
```bash
# Kill processes using the port (Windows)
netstat -ano | findstr :5001
taskkill /PID <PID> /F

# Kill processes using the port (macOS/Linux)
lsof -ti:5001 | xargs kill -9
```

---

#### Tests Pass Locally but Fail in CI

**Symptom**: Tests work on your machine but fail in GitHub Actions

**Possible causes**:
1. **Environment differences**: Check .NET version matches
2. **Missing dependencies**: Ensure all packages restored
3. **Timing issues**: Add appropriate waits for async operations
4. **External dependencies**: Mock external API calls

**Debug in CI**:
```yaml
- name: Run tests with verbose output
  run: dotnet test --verbosity detailed
```

---

#### SSL Certificate Errors

**Symptom**: Tests fail with SSL/TLS errors

**Solution**: The test server handles this automatically, but if needed:
```csharp
// In test setup
var clientOptions = new WebApplicationFactoryClientOptions
{
    AllowAutoRedirect = false,
    HandleCookies = false
};
_client = _factory.CreateClient(clientOptions);
```

---

### Getting Help

1. **Check documentation**: Review this guide and the [Development Guide](development-guide.md)
2. **Review existing tests**: Look at `ScriptGeneratorApiTests.cs` for examples
3. **Check GitHub Issues**: Search for similar problems
4. **Run with verbose output**: `dotnet test --verbosity detailed`
5. **Debug tests**: Use debugger in Visual Studio/VS Code/Rider

---

## Additional Resources

### Official Documentation

- **xUnit**: [https://xunit.net](https://xunit.net)
- **FluentAssertions**: [https://fluentassertions.com](https://fluentassertions.com)
- **ASP.NET Core Testing**: [https://docs.microsoft.com/aspnet/core/test/integration-tests](https://docs.microsoft.com/aspnet/core/test/integration-tests)
- **GitHub Actions**: [https://docs.github.com/actions](https://docs.github.com/actions)

### Project Documentation

- **[Development Guide](development-guide.md)** - Setup and development workflow
- **[API Reference](api-reference.md)** - Complete API documentation
- **[Integration Tests README](../src/PSW.IntegrationTests/README.md)** - Test project documentation

---

[← Back to Documentation Index](documentation.md)
