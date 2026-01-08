# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Package Script Writer (PSW) is a tool for generating Umbraco CMS installation scripts. It exists as both a **web application** (ASP.NET Core) and a **CLI tool** (.NET Global Tool). The codebase uses a shared library pattern where common business logic is extracted into `PSW.Shared` and consumed by both applications.

## Project Structure

The repository contains two separate solutions:

```
src/
├── PSW.sln                    # Web application solution
│   ├── PSW/                   # Main web application (ASP.NET Core)
│   ├── PSW.Shared/            # Shared business logic library
│   └── PSW.IntegrationTests/  # Integration tests for web app
│
├── PackageCliTool.sln         # CLI tool solution
│   ├── PackageCliTool/        # CLI tool (.NET Global Tool)
│   ├── PSW.Shared/            # Shared business logic library (same as above)
│   └── PackageCliTool.Tests/  # Unit tests for CLI
│
├── PSW.ApiDiagnostic/         # Diagnostic tool (standalone)
└── PSW.Shared/                # Shared library (referenced by both solutions)
```

### Key Architecture Pattern

**PSW.Shared** is the critical shared library containing:
- Services: `IScriptGeneratorService`, `IPackageService`, `IQueryStringService`, `IUmbracoVersionService`
- Models: All DTOs, view models, and API request/response objects
- Constants: Template definitions, default values
- Enums: Database types, template types
- Extensions: String and query collection helpers

Both the web app and CLI consume these shared services, ensuring consistent script generation logic.

## Common Development Commands

### Web Application (PSW)

```bash
# Run web app with hot reload
dotnet watch run --project ./src/PSW/

# Access at: https://localhost:5001
# Swagger UI at: https://localhost:5001/api/docs

# Run integration tests
dotnet test ./src/PSW.IntegrationTests/PSW.IntegrationTests.csproj

# Build for release
dotnet build ./src/PSW/PSW.csproj -c Release

# Publish for deployment
dotnet publish ./src/PSW/PSW.csproj -c Release -o ./publish

# Format code (automatically runs before build)
dotnet format ./src/PSW/PSW.csproj --severity warn --verbosity diagnostic
```

### CLI Tool (PackageCliTool)

```bash
# Build CLI tool
dotnet build ./src/PackageCliTool/PackageCliTool.csproj

# Run CLI locally (from project directory)
dotnet run --project ./src/PackageCliTool/

# Pack as NuGet package
dotnet pack ./src/PackageCliTool/PackageCliTool.csproj -c Release

# Install globally for testing
dotnet tool install --global --add-source ./src/PackageCliTool/bin/Release PackageScriptWriter.Cli

# Update existing installation
dotnet tool update --global --add-source ./src/PackageCliTool/bin/Release PackageScriptWriter.Cli

# Uninstall
dotnet tool uninstall --global PackageScriptWriter.Cli

# Run tests
dotnet test ./src/PackageCliTool.Tests/PackageCliTool.Tests.csproj
```

### Working with Both Solutions

```bash
# Build entire repository
dotnet build ./src/PSW.sln
dotnet build ./src/PackageCliTool.sln

# Run all tests (web + CLI)
dotnet test ./src/PSW.sln
dotnet test ./src/PackageCliTool.sln
```

## Architecture Insights

### Service Layer Pattern

All business logic lives in services registered via Dependency Injection:

**Web App (Program.cs)**:
```csharp
builder.Services.AddScoped<IScriptGeneratorService, ScriptGeneratorService>();
builder.Services.AddScoped<IPackageService, MarketplacePackageService>();
builder.Services.AddScoped<IQueryStringService, QueryStringService>();
builder.Services.AddScoped<IUmbracoVersionService, UmbracoVersionService>();
```

**CLI Tool (Program.cs)**:
```csharp
services.AddSingleton<IScriptGeneratorService, ScriptGeneratorService>();
services.AddSingleton<IPackageService, MarketplacePackageService>();
services.AddSingleton<IUmbracoVersionService, UmbracoVersionService>();
```

Note: Web app uses `Scoped` lifetime (per HTTP request), CLI uses `Singleton` (per application run).

### Key Services

1. **ScriptGeneratorService** (`PSW.Shared/Services/ScriptGeneratorService.cs`)
   - Core script generation logic (~327 LOC)
   - Generates bash/PowerShell scripts for Umbraco installation
   - Handles templates, packages, database config, Docker options
   - Used by both web and CLI

2. **MarketplacePackageService** (`PSW.Shared/Services/MarketplacePackageService.cs`)
   - Fetches packages from Umbraco Marketplace API
   - Retrieves package versions from NuGet.org
   - Uses `IMemoryCache` for performance (60-minute TTL)

3. **QueryStringService** (`PSW.Shared/Services/QueryStringService.cs`)
   - Converts between URL query strings and `PackagesViewModel`
   - Enables shareable configuration URLs

4. **UmbracoVersionService** (`PSW.Shared/Services/UmbracoVersionService.cs`)
   - Manages Umbraco version lifecycle data (LTS/STS status)
   - Hardcoded version data (not fetched from API)

### CLI-Specific Architecture

The CLI tool (`PackageCliTool/`) has additional services not in the shared library:

- **ApiClient** - HTTP client for calling the web API
- **CacheService** - File-based cache for offline operation
- **PackageSelector** - Interactive package selection UI (Spectre.Console)
- **ScriptExecutor** - Executes generated scripts
- **TemplateService** - Saves/loads script templates (YAML)
- **HistoryService** - Tracks command history
- **VersionCheckService** - Checks for CLI updates

The CLI has three workflow modes:
1. **InteractiveModeWorkflow** - Guided prompts with Spectre.Console UI
2. **CliModeWorkflow** - Direct command-line execution with flags
3. **TemplateWorkflow** - Template management (save/list/load)

### Web App-Specific Architecture

The web app (`PSW/`) uses:

- **View Components** - 10+ modular UI sections (PackageViewComponent, OptionsViewComponent, etc.)
- **Razor Pages** - Server-side rendering with Bootstrap 5
- **site.js** - 740+ lines of vanilla JavaScript for client interactions
- **SecurityHeadersMiddleware** - Custom middleware for security headers
- **Swagger/OpenAPI** - Interactive API documentation

### Controllers

**Web App**:
- `HomeController.cs` - Main MVC controller for page rendering
- `ScriptGeneratorApiController.cs` - REST API endpoints
- `CommunityTemplatesApiController.cs` - Community templates API
- `UUIController.cs` - Umbraco Unified Interface sandbox

### External API Integration

The application integrates with:
1. **NuGet.org API** - Package version lookups
2. **Umbraco Marketplace API** - Package metadata (500+ packages)

Both APIs are accessed via `HttpClientFactory` with proper connection pooling.

### Caching Strategy

**Web App**: Uses `IMemoryCache` (in-memory, 60-minute TTL)
- Cache keys: `all-packages`, `package-versions-{packageId}`, `umbraco-templates`
- Single-server architecture (doesn't scale horizontally)

**CLI Tool**: Uses file-based `CacheService` with 1-hour TTL
- Caches to user profile directory for offline operation
- Can be cleared with `psw --clear-cache`

### IPv4 Optimization

The CLI tool forces IPv4 connections to avoid IPv6 timeout issues (~42 seconds):

```csharp
.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    ConnectCallback = async (context, cancellationToken) =>
    {
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        await socket.ConnectAsync(context.DnsEndPoint, cancellationToken);
        return new NetworkStream(socket, ownsSocket: true);
    }
});
```

## Testing

### Integration Tests

Located in `src/PSW.IntegrationTests/`:
- Uses `WebApplicationFactory<Program>` for in-memory testing
- Tests all API endpoints (`/api/scriptgeneratorapi/*`)
- Uses xUnit + FluentAssertions

Run with:
```bash
dotnet test ./src/PSW.IntegrationTests/PSW.IntegrationTests.csproj --verbosity normal
```

### Unit Tests

Located in `src/PackageCliTool.Tests/`:
- Tests CLI-specific services and workflows

### API Testing

Interactive testing via Swagger UI:
1. Start app: `dotnet watch run --project ./src/PSW/`
2. Open: `https://localhost:5001/api/docs`

Or use REST Client with `Api Request/API Testing.http` file.

## Configuration

### Web App (appsettings.json)

```json
{
  "PSWConfig": {
    "MarketplaceApiUrl": "https://marketplace.umbraco.com",
    "NuGetApiUrl": "https://api.nuget.org/v3/index.json"
  }
}
```

### CLI Tool

Configuration in `PackageCliTool/appsettings.json` is embedded in the tool and deployed with it.

Enable verbose logging:
```bash
# Environment variable
PSW_VERBOSE=1 psw

# Or command flag
psw --verbose
```

## Important Patterns

### Template Dictionary

`PSW.Shared/Dictionaries/TemplateDictionary.cs` maps template short names to NuGet package IDs:
- Used for resolving "umbraco" → "Umbraco.Templates"
- Both web and CLI depend on this mapping

### Default Values

`PSW.Shared/Constants/DefaultValues.cs` defines defaults:
- Default project name: "MyProject"
- Default solution name: "MySolution"
- Default database: SQLite
- Default Umbraco version: Latest

### Database Types

Enum in `PSW.Shared/Enums/GlobalEnums.cs`:
- SQLite
- LocalDB
- SQLServer

### Script Generation Flow

1. User selects template + version
2. User selects packages + versions
3. User configures options (database, Docker, etc.)
4. `QueryStringService` encodes to URL (web) or stores in memory (CLI)
5. `ScriptGeneratorService.GenerateScript()` produces bash/PowerShell script
6. Script includes:
   - Template installation
   - Solution/project creation
   - Package additions
   - Optional unattended install
   - Optional Docker files

## Working with the Codebase

### Adding a New Package Source

To add a new package source beyond Umbraco Marketplace:
1. Add interface method to `IPackageService` in `PSW.Shared/`
2. Implement in `MarketplacePackageService.cs`
3. Update cache keys if needed
4. Update both web UI and CLI selector logic

### Adding a New Script Option

To add a new configuration option (e.g., new database type):
1. Add to `PackagesViewModel` in `PSW.Shared/Models/`
2. Update `ScriptGeneratorService.GenerateScript()` logic
3. Update web UI in `OptionsViewComponent`
4. Update CLI prompts in `InteractiveModeWorkflow`
5. Update query string encoding/decoding in `QueryStringService`

### Modifying Script Output

All script generation is in `ScriptGeneratorService.cs`:
- Uses `StringBuilder` for script assembly
- Separate methods for different sections (template, packages, unattended, Docker)
- Returns plain text (bash or PowerShell syntax)

### Stateless Design

The application is **intentionally stateless**:
- No database
- No user accounts
- Configuration encoded in URLs (web) or command flags (CLI)
- All data fetched from external APIs
- Perfect for sharing and CI/CD automation

### Maintaining the Changelog

**IMPORTANT**: When adding features or making changes, always update the CHANGELOG.md file.

Location: `src/PackageCliTool/CHANGELOG.md`

Process:
1. Complete the feature and ensure builds/tests pass
2. Add the change to the `[Unreleased]` section
3. Use the appropriate category:
   - `### Added` - New features
   - `### Changed` - Changes to existing functionality
   - `### Deprecated` - Soon-to-be removed features
   - `### Removed` - Removed features
   - `### Fixed` - Bug fixes
   - `### Security` - Security-related changes

Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) standard:
```markdown
## [Unreleased]

### Added
- **Feature Name** - Brief description of the feature

### Changed
- **Updated Behavior** - What changed and why
```

Example:
```markdown
## [Unreleased]

### Added
- **Exit Option in Interactive Mode** - Added "Exit" option to the main menu for graceful application exit
```

## Security Considerations

- **No user input stored** - All data is transient
- **Security headers** - Custom middleware adds HSTS, X-Frame-Options, CSP
- **Input validation** - Both client and server side
- **HTTPS enforced** - In production via HSTS
- **No secrets** - Application doesn't handle credentials (users paste into their own scripts)

## CI/CD

GitHub Actions workflows in `.github/workflows/`:
- `website-build-and-test.yml` - Builds web app and runs integration tests on PR
- Tests must pass before merge

## Common Gotchas

1. **Two separate solutions** - Changes to `PSW.Shared` affect both web and CLI
2. **Different lifetimes** - Web uses Scoped, CLI uses Singleton
3. **Cache differences** - Web uses IMemoryCache, CLI uses file-based cache
4. **Pre-build formatting** - Web app runs `dotnet format` before build (can be disabled with `RunPreBuildEvent=false`)
5. **CLI IPv4 forcing** - CLI has special HTTP handler to avoid IPv6 timeouts
6. **Partial Program class** - Web app has `public partial class Program {}` for integration test access

## Documentation

Extensive documentation in `.github/` directory:
- `architecture.md` - Detailed architecture diagrams
- `development-guide.md` - Setup and contribution guide
- `api-reference.md` - REST API documentation
- `services.md` - Service layer documentation
- `testing.md` - Testing strategies
- `cli-documentation.md` - CLI tool documentation

## Support

- Live site: https://psw.codeshare.co.uk
- Issues: https://github.com/prjseal/Package-Script-Writer/issues
- CLI on NuGet: https://www.nuget.org/packages/PackageScriptWriter.Cli/
