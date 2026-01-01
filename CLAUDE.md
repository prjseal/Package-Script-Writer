# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Package Script Writer (PSW) is a dual-mode tool that generates customized installation scripts for Umbraco CMS projects:
- **Web Application**: ASP.NET Core 10.0 Razor Pages application hosted at psw.codeshare.co.uk
- **CLI Tool**: .NET Global Tool (`psw`) distributed via NuGet for terminal-based automation

The application is intentionally stateless (no database) and uses external APIs (NuGet.org, Umbraco Marketplace) with aggressive caching.

## Development Commands

### Web Application

```bash
# Run with hot reload (recommended for development)
dotnet watch run --project ./src/PSW/

# Run without hot reload
dotnet run --project ./src/PSW/

# Build
dotnet build ./src/PSW/PSW.csproj

# Publish for production
dotnet publish ./src/PSW/PSW.csproj -c Release -o ./publish

# Format code
dotnet format ./src/PSW/PSW.csproj

# Trust development HTTPS certificate (first time setup)
dotnet dev-certs https --trust
```

**Access Points:**
- Web UI: https://localhost:5001
- Swagger API Docs: https://localhost:5001/api/docs

### Testing

```bash
# Run all integration tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "FullyQualifiedName~ScriptGeneratorApiTests"
```

**Test Project**: `src/PSW.IntegrationTests/` uses xUnit, ASP.NET Core Testing, FluentAssertions

### CLI Tool Development

```bash
# Run the CLI tool locally (from PackageCliTool directory)
dotnet run --project ./src/PackageCliTool/

# Pack the CLI tool for NuGet
dotnet pack ./src/PackageCliTool/PackageCliTool.csproj -c Release

# Install CLI tool globally (for testing)
dotnet tool install --global --add-source ./src/PackageCliTool/bin/Release PackageScriptWriter.Cli

# Uninstall CLI tool
dotnet tool uninstall --global PackageScriptWriter.Cli
```

### API Testing

Use the built-in Swagger UI at https://localhost:5001/api/docs (recommended), or use the REST Client extension with `API Requests/API Testing.http`.

## Architecture

### Three-Tier Structure

```
┌─────────────────────────────────────────────────┐
│              PSW (Web App)                       │
│  Controllers → View Components → Razor Pages    │
└──────────────────┬──────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────┐
│         PSW.Shared (Business Logic)             │
│  Services, Models, Extensions, Constants        │
└──────────────────▲──────────────────────────────┘
                   │
┌──────────────────┴──────────────────────────────┐
│          PackageCliTool (CLI)                    │
│  Workflows → Services → UI (Spectre.Console)    │
└─────────────────────────────────────────────────┘
```

### PSW.Shared Library (Core Business Logic)

This is the heart of the application, shared by both web and CLI projects:

**Services** (interface-based for DI):
- `IScriptGeneratorService` / `ScriptGeneratorService`: Generates shell scripts for Umbraco installation (327+ LOC)
- `IPackageService` / `MarketplacePackageService`: Fetches packages from Umbraco Marketplace and NuGet.org
- `IQueryStringService` / `QueryStringService`: Converts between query strings and view models for URL sharing
- `IUmbracoVersionService` / `UmbracoVersionService`: Manages Umbraco version lifecycle data and template handling

**Key Models**:
- `PackagesViewModel`: Main view model (20+ properties) containing all form state
- `GeneratorApiRequest`: API request DTO for script generation endpoint
- `PagedPackages`: Umbraco Marketplace feed response (500+ lines)
- `NugetPackageVersionFeed`: NuGet.org API response models

**Important Constants** (`src/PSW.Shared/Constants/`):
- `GlobalConstants`: Template names, Umbraco version mappings
- `DefaultValues`: Default project settings

**Version Handling**:
- Umbraco v15+ uses `@` separator (e.g., `Umbraco.Templates@15.0.0`) to avoid .NET 9 deprecation warnings
- Earlier versions use `::` separator (e.g., `Umbraco.Templates::14.3.0`)
- Logic extracted in helper methods within `ScriptGeneratorService` and `UmbracoVersionService`

### Web Application (PSW)

**Program.cs Setup**:
- Scoped service registration for: IScriptGeneratorService, IPackageService, IQueryStringService, IUmbracoVersionService
- In-memory caching with IMemoryCache
- HttpClientFactory for external API calls
- Swagger/OpenAPI with XML documentation
- SecurityHeadersMiddleware for security headers

**Controllers**:
- `HomeController`: MVC controller for page rendering with 60-minute cache TTL
- `ScriptGeneratorApiController`: RESTful API with three endpoints:
  - `POST /api/scriptgeneratorapi/generatescript` - Generates installation scripts
  - `POST /api/scriptgeneratorapi/getpackageversions` - Fetches NuGet package versions
  - `GET /api/scriptgeneratorapi/clearcache` - Clears in-memory cache
- `CommunityTemplatesApiController`: API for community template management

**View Components** (10+ components in `src/PSW/Components/`):
- Modular UI sections invoked via `@await Component.InvokeAsync("ComponentName", model)`
- Each component is self-contained with logic in `.cs` file and view in `Views/Shared/Components/`
- Examples: PackageViewComponent, OptionsViewComponent, InstallScriptViewComponent

**Caching Strategy**:
- IMemoryCache with 60-minute TTL
- Cache keys: `all-packages`, `package-versions-{packageId}`, `umbraco-templates`
- Provides 100x performance improvement over non-cached requests

**Frontend** (`wwwroot/js/site.js` - 740+ lines):
- Vanilla JavaScript (no frameworks)
- Main object: `psw` with methods for form handling, URL management, localStorage
- Integrates with Bootstrap 5 for UI components

### CLI Tool (PackageCliTool)

**Workflow-Based Architecture** (`src/PackageCliTool/Workflows/`):
- `InteractiveModeWorkflow`: Spectre.Console prompts for user interaction
- `CliModeWorkflow`: Command-line flag parsing and execution
- `TemplateWorkflow`: Template save/load/list operations
- `HistoryWorkflow`: Script generation history management
- `VersionsWorkflow`: Display Umbraco version information

**Services**:
- `ApiClient`: Wraps API calls to PSW web service with caching
- `PackageSelector`: Interactive package selection with search/filter
- `ScriptExecutor`: Clipboard operations and script execution
- `TemplateService`: Save/load user templates to `~/.psw/templates/`
- `HistoryService`: Persists generation history to `~/.psw/history.json`
- `CacheService`: File-based cache with TTL in `~/.psw/cache/`
- `CommunityTemplateService`: Loads community templates from GitHub
- `ResilientHttpClient`: HTTP client with exponential backoff retry logic
- `VersionCheckService`: Checks for CLI tool updates

**Validation Layer** (`src/PackageCliTool/Validation/`):
- `CommandValidator`: Validates command-line arguments and flags
- `InputValidator`: Validates user input (project names, emails, etc.)
- `TemplateValidator`: Validates template configurations

**UI Layer** (`src/PackageCliTool/UI/`):
- `InteractivePrompts`: Spectre.Console prompt wrappers
- `ConsoleDisplay`: Pretty-printed output with tables and panels
- `ConfigurationDisplay`: Displays script configuration before generation

**Models** (`src/PackageCliTool/Models/`):
- `CommandLineOptions`: Parsed CLI arguments with validation
- `ScriptModel`: API DTOs shared with web service
- `TemplateConfiguration` / `TemplateMetadata`: User template storage
- `HistoryEntry` / `ScriptHistory`: Generation history models
- `CachedData` / `CacheEntry`: Cache storage models

**Logging**:
- Serilog with file logging to `~/.psw/logs/`
- Verbose mode: `PSW_VERBOSE=1` or `--verbose` flag
- Logger setup in `src/PackageCliTool/Logging/LoggerSetup.cs`

**Special IPv4 Handling**:
- Custom SocketsHttpHandler forces IPv4 to avoid ~42 second IPv6 timeout on some networks
- Configured in Program.cs during HttpClient registration

## Important Patterns & Conventions

### Umbraco Version Separator Logic

**CRITICAL**: When modifying script generation or template handling:
- Umbraco v15+ requires `@` separator to avoid .NET 9 deprecation warnings for `::`
- Use `VersionHelper` or similar logic to determine separator based on version
- Example: `dotnet new install Umbraco.Templates@15.0.0` (v15+) vs `dotnet new install Umbraco.Templates::14.3.0` (v14 and earlier)
- This logic is in `ScriptGeneratorService` and `UmbracoVersionService`

### Dependency Injection

All services use constructor injection with interface-based contracts:
```csharp
public class ScriptGeneratorService : IScriptGeneratorService
{
    private readonly PSWConfig _pswConfig;
    private readonly IUmbracoVersionService _umbracoVersionService;

    public ScriptGeneratorService(
        IOptions<PSWConfig> pswConfig,
        IUmbracoVersionService umbracoVersionService)
    {
        _pswConfig = pswConfig.Value;
        _umbracoVersionService = umbracoVersionService;
    }
}
```

**Service Lifetimes**:
- Web App: All services are `Scoped` (per HTTP request)
- CLI Tool: All services are `Singleton` (shared across console app lifetime)

### View Component Pattern

View Components are invoked in Razor views:
```razor
@await Component.InvokeAsync("Package", new { model = Model })
```

Each component has:
- Logic class: `src/PSW/Components/PackageViewComponent.cs`
- View: `src/PSW/Views/Shared/Components/Package/Default.cshtml`

### Command-Line Flag Validation

CLI flags are validated in `CommandValidator.cs`:
- Short flags can be single char (e.g., `-p`) or multi-char (e.g., `-da` for `--default --auto-run`)
- Long flags use `--` prefix (e.g., `--packages`, `--project-name`)
- Package format: `packageId|version` or just `packageId` for latest

### Error Handling

- Web App: Development shows detailed errors, Production shows generic error page via `/Home/Error`
- CLI Tool: `ErrorHandler.Handle()` centralizes exception handling with Spectre.Console formatted output

### Stateless Architecture

**No database required**:
- All data fetched from external APIs (NuGet.org, Umbraco Marketplace)
- Configuration state encoded in URL for sharing
- CLI tool uses local file storage for templates/history/cache

## Code Organization

### Solution Structure

```
src/
├── PSW/                        # Web application (ASP.NET Core)
├── PSW.Shared/                 # Shared business logic library
├── PackageCliTool/             # CLI tool (.NET Global Tool)
├── PSW.IntegrationTests/       # Integration tests (xUnit)
├── PSW.ApiDiagnostic/          # API diagnostic tool
├── PSW.sln                     # Web app solution
└── PackageCliTool.sln          # CLI tool solution
```

### File Naming Conventions

- Services: `I{Name}Service.cs` (interface), `{Name}Service.cs` (implementation)
- View Components: `{Name}ViewComponent.cs`
- Controllers: `{Name}Controller.cs` for MVC, `{Name}ApiController.cs` for API
- Models: Descriptive names ending in `ViewModel`, `Request`, `Response`, or `Model`

### C# Coding Standards

- PascalCase: Classes, methods, properties, interfaces
- camelCase: Local variables, parameters
- _camelCase: Private fields
- UPPER_CASE: Constants
- Interface prefix: `I` (e.g., `IScriptGeneratorService`)

## Testing Approach

### Integration Tests

Location: `src/PSW.IntegrationTests/`

**Test Coverage**:
- API health checks
- Cache clearing functionality
- Script generation with various configurations
- Package version retrieval
- Error handling and validation

**Test Pattern**:
```csharp
public class ScriptGeneratorApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ScriptGeneratorApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GenerateScript_WithValidRequest_ReturnsScript()
    {
        // Arrange
        var request = new { model = new { ... } };

        // Act
        var response = await _client.PostAsJsonAsync("/api/scriptgeneratorapi/generatescript", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("script").GetString().Should().NotBeNullOrEmpty();
    }
}
```

**CustomWebApplicationFactory**: Configures test server with in-memory services

### Continuous Integration

GitHub Actions workflow: `.github/workflows/website-build-and-test.yml`
- Runs on every PR
- Executes all tests
- Blocks merge if tests fail

## Common Development Scenarios

### Adding New Template Support

1. Update `GlobalConstants.cs` with new template name
2. Add template-specific logic in `ScriptGeneratorService.GenerateScript()`
3. Update `UmbracoVersionService` if version handling differs
4. Update UI in `OptionsViewComponent.cshtml`
5. Test script generation with new template
6. Add integration test for new template

### Adding New API Endpoint

1. Add method to appropriate controller (`ScriptGeneratorApiController`, etc.)
2. Add XML documentation comments for Swagger
3. Add service method if business logic is complex
4. Add integration test in `PSW.IntegrationTests/`
5. Test via Swagger UI at https://localhost:5001/api/docs
6. Add example to `API Requests/API Testing.http`

### Modifying Script Generation Logic

1. Edit methods in `ScriptGeneratorService.cs` (in PSW.Shared)
2. Be mindful of Umbraco version separator logic (@ vs ::)
3. Update integration tests to cover new scenarios
4. Test with multiple configurations (different templates, versions, packages)
5. Verify output format in both web UI and CLI tool

### Adding New CLI Workflow

1. Create workflow class in `src/PackageCliTool/Workflows/`
2. Inherit from base workflow pattern or create standalone
3. Add command detection in `CommandLineOptions.cs`
4. Wire up in `Program.cs` Main method
5. Add help text to `ConsoleDisplay.DisplayHelp()`
6. Test in both interactive and CLI modes

### Working with External APIs

**NuGet.org API**:
- Base URL: `https://api.nuget.org/v3-flatcontainer/`
- Used by `MarketplacePackageService`
- Returns JSON for package versions
- Caching is critical (60-minute TTL)

**Umbraco Marketplace API**:
- Fetches 500+ packages
- Paged results in `PagedPackages` model
- Cached with key `all-packages`

### Debugging Tips

1. **Cache Issues**: Clear cache via `curl https://localhost:5001/api/scriptgeneratorapi/clearcache`
2. **Hot Reload Not Working**: Ensure using `dotnet watch run`, check file watcher limits on Linux
3. **SSL Certificate Errors**: Run `dotnet dev-certs https --trust`
4. **CLI Verbose Logging**: Set `PSW_VERBOSE=1` or use `--verbose` flag
5. **Browser DevTools**: Use F12 Network tab to inspect API requests/responses

## Documentation

Comprehensive documentation in `.github/` directory:
- `architecture.md` - System architecture and design patterns
- `services.md` - Business logic layer documentation
- `api-reference.md` - Complete REST API documentation
- `development-guide.md` - Setup, testing, and contributing
- `cli-documentation.md` - Complete CLI tool documentation

## Security Considerations

- SecurityHeadersMiddleware adds X-Frame-Options, CSP, etc.
- HTTPS/HSTS enforced in production
- No sensitive data storage (stateless)
- Input validation on client and server side
- Regular dependency updates

## Performance Notes

- Caching provides 100x improvement over non-cached requests
- IPv4-only HTTP client avoids 42-second IPv6 timeouts
- Resilient HTTP client with exponential backoff in CLI tool
- Connection pooling via HttpClientFactory
