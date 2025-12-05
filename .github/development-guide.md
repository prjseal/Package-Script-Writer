# Development Guide

Complete guide for setting up, developing, testing, and contributing to the Package Script Writer project.

## Table of Contents
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)
- [Testing](#testing)
- [Building and Deployment](#building-and-deployment)
- [Code Style](#code-style)
- [Contributing](#contributing)
- [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Required Software

| Software | Minimum Version | Purpose |
|----------|----------------|---------|
| .NET SDK | 9.0+ | Application runtime |
| Git | 2.30+ | Version control |
| Visual Studio / VS Code / Rider | Latest | IDE |

### Optional Tools

| Tool | Purpose |
|------|---------|
| REST Client (VS Code Extension) | API testing |
| Postman | API testing |
| Docker | Container testing |
| SQL Server | Database testing (if needed) |

---

### Installation

#### Install .NET SDK

**Windows**:
```powershell
winget install Microsoft.DotNet.SDK.9
```

**macOS**:
```bash
brew install dotnet
```

**Linux (Ubuntu)**:
```bash
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 10.0
```

**Verify Installation**:
```bash
dotnet --version
# Output: 10.0.x
```

---

#### Install Git

**Windows**:
```powershell
winget install Git.Git
```

**macOS**:
```bash
brew install git
```

**Linux**:
```bash
sudo apt-get install git
```

---

## Getting Started

### 1. Clone Repository

```bash
git clone https://github.com/prjseal/Package-Script-Writer.git
cd Package-Script-Writer
```

---

### 2. Restore Dependencies

```bash
cd src/PSW
dotnet restore
```

**Output**:
```
Restore completed in 2.5 sec for Package-Script-Writer.csproj
```

---

### 3. Run Application

#### Option A: Using dotnet CLI

```bash
dotnet run --project ./src/PSW/PSW.csproj
```

#### Option B: Using dotnet watch (recommended for development)

```bash
dotnet watch run --project ./src/PSW/PSW.csproj
```

**Benefits of `dotnet watch`**:
- Auto-reloads on file changes
- Hot reload for Razor pages
- Faster iteration

---

### 4. Access Application

Open browser and navigate to:
- **HTTPS**: `https://localhost:5001`
- **HTTP**: `http://localhost:5000` (redirects to HTTPS)

**Expected Output**:
```
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

---

## Development Workflow

### Project Structure

```
src/PSW/
├── Components/          # View Components
├── Controllers/         # MVC & API Controllers
├── Models/             # Data models
├── Services/           # Business logic
├── Views/              # Razor views
├── wwwroot/            # Static files
│   ├── css/
│   ├── js/
│   └── images/
├── appsettings.json    # Configuration
└── Program.cs          # Entry point
```

---

### Common Development Tasks

#### Add New Package Source

1. Update `MarketplacePackageService.cs`
2. Add new API endpoint configuration
3. Update cache keys
4. Test API integration

#### Add New Template Support

1. Update `TemplateDictionary.cs`
2. Add template-specific logic in `ScriptGeneratorService.cs`
3. Update UI in `OptionsViewComponent.cshtml`
4. Test script generation

#### Modify Script Generation

1. Edit methods in `ScriptGeneratorService.cs`
2. Update tests
3. Verify output format
4. Test with multiple configurations

---

## Testing

### Integration Tests

The project includes a comprehensive integration test suite using **xUnit**, **Microsoft.AspNetCore.Mvc.Testing**, **HttpClient**, and **FluentAssertions**. Integration tests validate API endpoints in a realistic environment by spinning up a test server and making actual HTTP requests.

#### Running Integration Tests

**Using .NET CLI** (recommended):
```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "FullyQualifiedName~ScriptGeneratorApiTests"
```

**Using Visual Studio**:
1. Open Test Explorer (Test → Test Explorer)
2. Click "Run All" or select specific tests

**Using Visual Studio Code**:
1. Install the .NET Test Explorer extension
2. Tests appear in Test Explorer panel
3. Click play button to run tests

---

#### Integration Test Coverage

The current test suite (`PSW.IntegrationTests` project) covers:

- ✅ **GET /api/ScriptGeneratorApi/test** - API health check
- ✅ **GET /api/ScriptGeneratorApi/clearcache** - Cache clearing functionality
- ✅ **POST /api/ScriptGeneratorApi/generatescript** - Script generation with valid data
- ✅ **POST /api/ScriptGeneratorApi/generatescript** - Script generation with empty request (error handling)
- ✅ **POST /api/ScriptGeneratorApi/getpackageversions** - Package version retrieval

**Test Project Structure**:
```
src/PSW.IntegrationTests/
├── ScriptGeneratorApiTests.cs          # Main test class
├── CustomWebApplicationFactory.cs      # Test server configuration
└── GlobalUsings.cs                     # Common using statements
```

**Example Test**:
```csharp
[Fact]
public async Task GenerateScript_WithValidRequest_ReturnsScript()
{
    // Arrange
    var request = new
    {
        model = new
        {
            templateName = "Umbraco.Templates",
            templateVersion = "14.3.0",
            projectName = "TestProject"
        }
    };

    // Act
    var response = await _client.PostAsJsonAsync(
        "/api/ScriptGeneratorApi/generatescript", request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var result = await response.Content.ReadFromJsonAsync<JsonElement>();
    result.GetProperty("script").GetString().Should().NotBeNullOrEmpty();
}
```

**See [Integration Tests README](../src/PSW.IntegrationTests/README.md) for detailed information on writing tests.**

---

#### Continuous Integration with GitHub Actions

Every pull request automatically runs all tests via **GitHub Actions**:

**Workflow**: `.github/workflows/integration-tests.yml`

**Features**:
- 🔄 Runs on every pull request
- ✅ Builds the solution
- 🧪 Executes all integration tests
- 📊 Reports test results
- 🚫 Blocks PR merge if tests fail

**Workflow Triggers**:
```yaml
on:
  pull_request:
    branches: [ main, 'claude/**' ]
  workflow_dispatch:
```

**Manual Workflow Run**:
```bash
# Via GitHub CLI
gh workflow run integration-tests.yml
```

**View Test Results**:
1. Go to the Pull Request on GitHub
2. Check "Checks" tab
3. View "Integration Tests" workflow results

---

### API Testing

#### Using Swagger UI (Recommended)

The easiest way to test the API is through the built-in **Swagger UI** with full OpenAPI documentation:

1. Start the application: `dotnet watch run --project ./src/PSW/`
2. Navigate to: [https://localhost:5001/api/docs](https://localhost:5001/api/docs)
3. Click on any endpoint to expand it
4. Click "Try it out" button
5. Fill in the parameters
6. Click "Execute" to send the request
7. View the response directly in the browser

**Swagger UI Features**:
- 📖 Interactive API documentation with OpenAPI annotations
- 🧪 Built-in request testing
- 📝 Request/response examples with schemas
- 🔍 Model schema exploration
- 📄 Complete API specification download

---

#### Using REST Client (VS Code)

1. Install **REST Client** extension
2. Open file: `Api Request/API Testing.http`

```http
### Test Generate Script
POST https://localhost:5001/api/scriptgeneratorapi/generatescript
Content-Type: application/json

{
  "model": {
    "templateName": "Umbraco.Templates",
    "templateVersion": "14.3.0",
    "projectName": "TestProject"
  }
}

### Test Get Package Versions
POST https://localhost:5001/api/scriptgeneratorapi/getpackageversions
Content-Type: application/json

{
  "packageId": "Umbraco.Community.BlockPreview",
  "includePrerelease": false
}

### Test Clear Cache
GET https://localhost:5001/api/scriptgeneratorapi/clearcache
```

3. Run application: `dotnet watch run --project ./src/PSW/`
4. Click "Send Request" in VS Code

---

#### Using cURL

**Test Generate Script**:
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

**Note**: `-k` flag ignores SSL certificate validation (development only)

---

#### Using Postman

1. Import collection from `Api Request/` folder (if available)
2. Set base URL: `https://localhost:5001`
3. Configure to ignore SSL errors (Settings → SSL certificate verification → OFF)
4. Run requests

---

### Manual Testing Checklist

#### Functional Testing

- [ ] Template selection changes version dropdown
- [ ] Package selection loads version dropdown
- [ ] Search filters packages correctly
- [ ] Copy button copies script to clipboard
- [ ] Save configuration to localStorage works
- [ ] Restore configuration from localStorage works
- [ ] URL updates on form changes
- [ ] Loading configuration from URL works
- [ ] Docker options appear/disappear based on template
- [ ] Unattended install options appear/disappear
- [ ] One-liner output formats correctly
- [ ] Remove comments option works
- [ ] All Umbraco versions generate valid scripts
- [ ] All integration tests pass

#### Browser Testing

- [ ] Chrome (latest)
- [ ] Firefox (latest)
- [ ] Safari (latest)
- [ ] Edge (latest)
- [ ] Mobile Chrome
- [ ] Mobile Safari

---

### Writing New Integration Tests

To add new integration tests to the `PSW.IntegrationTests` project:

**1. Create Test Class**:
```csharp
public class MyNewApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public MyNewApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task MyEndpoint_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var request = new { /* test data */ };

        // Act
        var response = await _client.PostAsJsonAsync("/api/myendpoint", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

**2. Use FluentAssertions for readable assertions**:
```csharp
// Instead of:
Assert.Equal(HttpStatusCode.OK, response.StatusCode);

// Use:
response.StatusCode.Should().Be(HttpStatusCode.OK);
response.Content.Should().NotBeNull();
result.Should().Contain("expected text");
```

**3. Test both success and failure scenarios**:
```csharp
[Fact]
public async Task Endpoint_WithInvalidData_ReturnsBadRequest()
{
    var response = await _client.PostAsJsonAsync("/api/endpoint", null);
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
}
```

---

### Testing Best Practices

1. **Use IClassFixture**: Share `WebApplicationFactory` across tests for better performance
2. **Test behavior, not implementation**: Focus on API contracts and expected responses
3. **Use realistic data**: Test with data representing actual usage scenarios
4. **Isolate tests**: Each test should be independent
5. **Use meaningful names**: `MethodName_Scenario_ExpectedResult`
6. **Test error cases**: Verify proper error handling
7. **Run tests frequently**: Use `dotnet watch test` during development
8. **Check CI results**: Ensure tests pass in GitHub Actions

---

## Building and Deployment

### Development Build

```bash
dotnet build ./src/PSW/PSW.csproj
```

**Output**: `bin/Debug/net9.0/`

---

### Production Build

```bash
dotnet publish ./src/PSW/PSW.csproj -c Release -o ./publish
```

**Output**: `./publish/` (ready for deployment)

**Build Artifacts**:
- PSW.dll
- appsettings.json
- appsettings.Production.json
- wwwroot/ (static files)
- Dependencies

---

### Docker Deployment (Future)

**Dockerfile Example**:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/PSW/PSW.csproj", "src/PSW/"]
RUN dotnet restore "src/PSW/PSW.csproj"
COPY . .
WORKDIR "/src/src/PSW"
RUN dotnet build "PSW.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PSW.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PSW.dll"]
```

**Build Docker Image**:
```bash
docker build -t psw:latest .
```

**Run Container**:
```bash
docker run -d -p 8080:80 -p 8443:443 --name psw psw:latest
```

---

### Deployment Targets

#### Azure App Service

1. Create App Service (Linux, .NET 9)
2. Configure deployment (GitHub Actions, Azure DevOps)
3. Set environment variables
4. Deploy

**Azure CLI**:
```bash
az webapp create --name psw --resource-group myResourceGroup --plan myAppServicePlan --runtime "DOTNETCORE:9.0"
az webapp deployment source config --name psw --resource-group myResourceGroup --repo-url https://github.com/prjseal/Package-Script-Writer --branch main
```

#### IIS (Windows)

1. Install ASP.NET Core Hosting Bundle
2. Create IIS site
3. Copy published files to site directory
4. Configure application pool (.NET CLR Version: No Managed Code)

#### Linux (Nginx)

1. Install .NET Runtime
2. Configure systemd service
3. Setup Nginx reverse proxy
4. Enable and start service

---

## Code Style

### Formatting

**Auto-format on Build**:
```xml
<Target Name="PreBuild" BeforeTargets="PreBuildEvent">
  <Exec Command="dotnet format" />
</Target>
```

**Manual Format**:
```bash
dotnet format ./src/PSW/PSW.csproj
```

---

### C# Coding Standards

**Naming Conventions**:
- PascalCase: Classes, methods, properties
- camelCase: Local variables, parameters
- _camelCase: Private fields
- UPPER_CASE: Constants

**Example**:
```csharp
public class ScriptGeneratorService : IScriptGeneratorService
{
    private readonly PSWConfig _pswConfig;
    private const int DEFAULT_TIMEOUT = 30;

    public string GenerateScript(PackagesViewModel model)
    {
        var outputList = new List<string>();
        // ...
    }
}
```

---

### JavaScript Coding Standards

**Naming Conventions**:
- camelCase: Variables, functions
- PascalCase: Constructors (not used in current code)

**Example**:
```javascript
var psw = {
    controls: {
        templateName: document.getElementById('TemplateName')
    },
    updateOutput: function() {
        var model = psw.buildModelFromForm();
        // ...
    }
};
```

---

### EditorConfig

**File**: `.editorconfig` (recommended)

```ini
root = true

[*]
charset = utf-8
indent_style = space
indent_size = 4
insert_final_newline = true
trim_trailing_whitespace = true

[*.{cs,csx}]
csharp_new_line_before_open_brace = all
csharp_indent_case_contents = true
csharp_space_after_cast = false

[*.{js,json}]
indent_size = 2

[*.{html,cshtml}]
indent_size = 2
```

---

## Contributing

### How to Contribute

1. **Raise an Issue** (if one doesn't exist)
   - Bug reports: Include steps to reproduce, expected vs actual behavior
   - Feature requests: Describe the feature and use case

2. **Discuss Before Coding**
   - Comment on the issue to discuss approach
   - Wait for maintainer feedback
   - Ensure feature aligns with project goals

3. **Fork Repository**
   ```bash
   # Fork via GitHub UI
   git clone https://github.com/YOUR_USERNAME/Package-Script-Writer.git
   ```

4. **Create Feature Branch**
   ```bash
   git checkout -b feature/my-new-feature
   # or
   git checkout -b fix/issue-123
   ```

5. **Make Changes**
   - Follow code style guidelines
   - Add comments for complex logic
   - Update documentation if needed

6. **Test Changes**
   - Test manually
   - Run existing tests (if any)
   - Verify in multiple browsers

7. **Commit Changes**
   ```bash
   git add .
   git commit -m "Add feature: description of feature"
   ```

   **Commit Message Format**:
   - `feat: Add support for community templates`
   - `fix: Correct Docker version detection`
   - `docs: Update API documentation`
   - `refactor: Simplify script generation logic`
   - `test: Add tests for QueryStringService`

8. **Push to Fork**
   ```bash
   git push origin feature/my-new-feature
   ```

9. **Create Pull Request**
   - Go to original repository
   - Click "New Pull Request"
   - Select your fork and branch
   - Fill in PR template:
     - What problem does this solve?
     - How did you fix it?
     - What is the result?
   - Link related issue: "Fixes #123"

10. **Review Process**
    - Maintainer will review
    - Address feedback if requested
    - Once approved, PR will be merged

---

### Pull Request Checklist

- [ ] Issue exists and is linked to PR
- [ ] Code follows project style
- [ ] Changes are tested manually
- [ ] Documentation updated (if needed)
- [ ] Commit messages are clear
- [ ] PR description explains changes
- [ ] No merge conflicts
- [ ] Ready for review

---

### Code Review Guidelines

**Reviewers check for**:
- Correct functionality
- Code quality and maintainability
- Performance implications
- Security concerns
- Test coverage
- Documentation completeness

---

## Troubleshooting

### Common Issues

#### Port Already in Use

**Error**:
```
Failed to bind to address https://127.0.0.1:5001: address already in use.
```

**Solution**:
```bash
# Find process using port 5001
lsof -i :5001  # macOS/Linux
netstat -ano | findstr :5001  # Windows

# Kill process
kill -9 <PID>  # macOS/Linux
taskkill /PID <PID> /F  # Windows

# Or run on different port
dotnet run --urls "https://localhost:5555"
```

---

#### SSL Certificate Errors

**Error**:
```
Unable to configure HTTPS endpoint. No server certificate was specified.
```

**Solution**:
```bash
# Trust development certificate
dotnet dev-certs https --trust

# Clean and recreate
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

---

#### Package Restore Fails

**Error**:
```
Unable to resolve package dependencies.
```

**Solution**:
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore with verbose output
dotnet restore --verbosity detailed
```

---

#### Hot Reload Not Working

**Symptoms**: Changes not reflected without manual restart

**Solution**:
1. Ensure using `dotnet watch run`
2. Check file watchers limit (Linux):
   ```bash
   # Increase inotify limit
   echo fs.inotify.max_user_watches=524288 | sudo tee -a /etc/sysctl.conf
   sudo sysctl -p
   ```
3. Restart `dotnet watch`

---

#### Cache Issues

**Symptoms**: Stale package data, versions not updating

**Solution**:
```bash
# Call cache clear endpoint
curl https://localhost:5001/api/scriptgeneratorapi/clearcache

# Or restart application
```

---

### Getting Help

1. **Documentation**: Check this guide and other documentation files
2. **Issues**: Search existing issues on GitHub
3. **Create Issue**: If problem persists, create new issue with:
   - Clear description
   - Steps to reproduce
   - Expected vs actual behavior
   - Environment details (.NET version, OS)

---

### Development Tips

1. **Use Hot Reload**: `dotnet watch run` for faster iteration
2. **Browser DevTools**: Use F12 for JavaScript debugging
3. **Network Tab**: Inspect API requests/responses
4. **Breakpoints**: Use debugger in Visual Studio/VS Code/Rider
5. **Logging**: Add temporary logging for debugging
6. **Cache Clearing**: Clear cache when testing package versions

---

## Additional Resources

### Official Documentation

- **ASP.NET Core**: https://docs.microsoft.com/aspnet/core
- **.NET CLI**: https://docs.microsoft.com/dotnet/core/tools
- **C# Language**: https://docs.microsoft.com/dotnet/csharp
- **Razor Pages**: https://docs.microsoft.com/aspnet/core/razor-pages

### Umbraco Resources

- **Umbraco Documentation**: https://docs.umbraco.com
- **Umbraco Marketplace**: https://marketplace.umbraco.com
- **Umbraco Templates**: https://www.nuget.org/packages/Umbraco.Templates

### Tools & Extensions

- **VS Code Extensions**:
  - C# (Microsoft)
  - REST Client (Huachao Mao)
  - GitLens (GitKraken)
  - Prettier (Code formatter)

- **Visual Studio Extensions**:
  - ReSharper (JetBrains)
  - Web Essentials
  - Productivity Power Tools

---

[← Back to Documentation Index](documentation.md)
