# Services

The service layer contains all business logic for the Package Script Writer application. Services are registered with **Scoped** lifetime in the dependency injection container.

## Table of Contents
- [Service Overview](#service-overview)
- [ScriptGeneratorService](#scriptgeneratorservice)
- [MarketplacePackageService](#marketplacepackageservice)
- [QueryStringService](#querystringservice)
- [UmbracoVersionService](#umbracoversionservice)
- [Service Registration](#service-registration)

---

## Service Overview

| Service | Interface | Purpose | Lines of Code |
|---------|-----------|---------|---------------|
| ScriptGeneratorService | IScriptGeneratorService | Generates installation scripts | 327 |
| MarketplacePackageService | IPackageService | Fetches package data from APIs | 150+ |
| QueryStringService | IQueryStringService | URL query string handling | 100+ |
| UmbracoVersionService | IUmbracoVersionService | Version lifecycle management | 80+ |

All services follow SOLID principles with clear separation of concerns and interface-based contracts.

---

## ScriptGeneratorService

**File**: `PSW/PSW/Services/ScriptGeneratorService.cs`

**Purpose**: Generates shell scripts for Umbraco project installation based on user selections.

### Dependencies

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

### Public Methods

#### GenerateScript

**Signature**:
```csharp
public string GenerateScript(PackagesViewModel model)
```

**Description**: Main entry point that orchestrates the entire script generation process.

**Flow**:
1. Generates Umbraco template installation commands
2. Creates solution file commands (if enabled)
3. Creates project with unattended install options
4. Adds project to solution
5. Generates Docker Compose commands (if enabled)
6. Adds starter kit package
7. Adds selected packages
8. Generates run command
9. Applies output formatting (comments, one-liner)

**Returns**: Complete installation script as a string

**Example Output**:
```bash
# Ensure we have the version specific Umbraco templates
dotnet new install Umbraco.Templates::14.3.0 --force

# Create solution/project
dotnet new sln --name "MySolution"
dotnet new umbraco --force -n "MyProject" --friendly-name "Admin" --email "admin@example.com" --password "Password123" --development-database-type SQLite
dotnet sln add "MyProject"

#Add Packages
dotnet add "MyProject" package Umbraco.Community.BlockPreview --version 1.6.0

dotnet run --project "MyProject"
#Running
```

---

#### GenerateUmbracoTemplatesSectionScript

**Signature**:
```csharp
public List<string> GenerateUmbracoTemplatesSectionScript(PackagesViewModel model)
```

**Description**: Generates commands to install Umbraco or community templates.

**Logic**:
- Handles LTS version replacement (converts "LTS" to actual version number)
- Uses `dotnet new install` for recent versions
- Uses `dotnet new -i` for legacy versions (9.x, 10.x)
- Supports specific version or latest

**Example Output**:
```bash
# Ensure we have the version specific Umbraco templates
dotnet new install Umbraco.Templates::14.3.0 --force
```

---

#### GenerateCreateSolutionFileScript

**Signature**:
```csharp
public List<string> GenerateCreateSolutionFileScript(PackagesViewModel model)
```

**Description**: Generates command to create a solution file.

**Conditions**: Only generates output if `model.CreateSolutionFile` is true

**Example Output**:
```bash
# Create solution/project
dotnet new sln --name "MySolution"
```

---

#### GenerateCreateProjectScript

**Signature**:
```csharp
public List<string> GenerateCreateProjectScript(PackagesViewModel model)
```

**Description**: Generates the project creation command with all configuration options.

**Features**:
- Supports unattended install with database configuration
- Handles multiple database types (SQLite, LocalDB, SQL Server, SQL Azure, SQLCE)
- Version-specific logic for different Umbraco versions
- Docker support flag (`--add-docker`)
- Special handling for v10 RC versions

**Database Type Handling**:

| Database Type | Umbraco < 10 | Umbraco >= 10 |
|--------------|--------------|---------------|
| SQLite | Connection string | `--development-database-type SQLite` |
| LocalDB | Connection string | `--development-database-type LocalDB` |
| SQL Server | Connection string | Connection string |
| SQL Azure | Connection string | Connection string |
| SQLCE | Connection string (v9 only) | Not supported |

**Example Output**:
```bash
dotnet new umbraco --force -n "MyProject" --add-docker --friendly-name "Admin User" --email "admin@example.com" --password "SuperSecret123" --development-database-type SQLite
```

---

#### GenerateAddProjectToSolutionScript

**Signature**:
```csharp
public List<string> GenerateAddProjectToSolutionScript(PackagesViewModel model)
```

**Description**: Generates command to add the project to the solution file.

**Conditions**: Only generates if both `CreateSolutionFile` is true and `SolutionName` is provided

**Example Output**:
```bash
dotnet sln add "MyProject"
```

---

#### GenerateAddStarterKitScript

**Signature**:
```csharp
public List<string> GenerateAddStarterKitScript(PackagesViewModel model, bool renderPackageName)
```

**Description**: Generates command to add a starter kit package.

**Parameters**:
- `model`: View model with starter kit settings
- `renderPackageName`: If true, includes project name in command

**Example Output**:
```bash
#Add starter kit
dotnet add "MyProject" package Umbraco.TheStarterKit
```

---

#### GenerateAddPackagesScript

**Signature**:
```csharp
public List<string> GenerateAddPackagesScript(PackagesViewModel model, bool renderPackageName)
```

**Description**: Generates commands to add all selected NuGet packages.

**Package String Format**: `PackageId|Version,PackageId|Version|--prerelease`

**Features**:
- Parses package string to extract package ID and version
- Skips starter kit if already added as starter kit
- Supports prerelease flag
- Handles version specification

**Example Output**:
```bash
#Add Packages
dotnet add "MyProject" package Umbraco.Community.BlockPreview --version 1.6.0
dotnet add "MyProject" package Diplo.GodMode --version 3.0.3
#Ignored Umbraco.TheStarterKit as it was added as a starter kit
```

---

#### GenerateRunProjectScript

**Signature**:
```csharp
public List<string> GenerateRunProjectScript(PackagesViewModel model, bool renderPackageName)
```

**Description**: Generates the final command to run the project.

**Example Output**:
```bash
dotnet run --project "MyProject"
#Running
```

---

#### GenerateAddDockerComposeScript

**Signature**:
```csharp
public List<string> GenerateAddDockerComposeScript(PackagesViewModel model)
```

**Description**: Generates command to add Docker Compose support.

**Conditions**: Only for Umbraco templates with Docker enabled and appropriate version

**Example Output**:
```bash
#Add Docker Compose
dotnet new umbraco-compose -P "MyProject"
```

---

## MarketplacePackageService

**File**: `PSW/PSW/Services/MarketplacePackageService.cs`

**Purpose**: Fetches package data from Umbraco Marketplace and NuGet.org with caching.

### Dependencies

```csharp
public class MarketplacePackageService : IPackageService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public MarketplacePackageService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
}
```

### Public Methods

#### GetAllPackages

**Signature**:
```csharp
public async Task<PagedPackages> GetAllPackages(
    IMemoryCache memoryCache,
    int cacheDurationMinutes)
```

**Description**: Retrieves all packages from Umbraco Marketplace with caching.

**Caching**:
- Cache Key: `all-packages`
- TTL: 60 minutes (configurable)
- Eviction: Absolute expiration

**API Endpoint**: `https://marketplace.umbraco.com/umbraco/api/marketplaceapi/getallpackages`

**Response Processing**:
1. Checks in-memory cache first
2. If cache miss, calls Marketplace API
3. Deserializes JSON response to `PagedPackages` model
4. Stores in cache for future requests
5. Returns package list

**Example Response Structure**:
```json
{
  "count": 150,
  "next": null,
  "packages": [
    {
      "id": "umbraco-community-blockpreview",
      "name": "Umbraco Community Block Preview",
      "description": "Preview Umbraco Block content",
      "owner": "rickbutterfield",
      "icon": "https://...",
      "tags": ["backoffice", "editor"]
    }
  ]
}
```

---

#### GetNugetPackageVersions

**Signature**:
```csharp
public async Task<List<string>> GetNugetPackageVersions(
    string packageId,
    bool includePrerelease,
    IMemoryCache memoryCache,
    int cacheDurationMinutes)
```

**Description**: Fetches available versions for a specific NuGet package.

**Caching**:
- Cache Key: `package-versions-{packageId}`
- TTL: 60 minutes (configurable)

**API Endpoints**:

1. **Prerelease Versions**:
   ```
   GET https://api.nuget.org/v3-flatcontainer/{packageId}/index.json
   ```
   Returns: All versions including prerelease

2. **Stable Versions Only**:
   ```
   GET https://azuresearch-usnc.nuget.org/query?q={packageId}&prerelease=false
   ```
   Returns: Stable versions only

**Response Processing**:
1. Checks cache for existing versions
2. If cache miss, calls appropriate NuGet API
3. Parses JSON response to extract version array
4. Stores in cache
5. Returns list of version strings

**Example Response**:
```json
{
  "versions": [
    "1.0.0",
    "1.1.0",
    "1.2.0",
    "2.0.0-beta",
    "2.0.0"
  ]
}
```

---

## QueryStringService

**File**: `PSW/PSW/Services/QueryStringService.cs`

**Purpose**: Handles bidirectional conversion between URL query strings and view models.

### Public Methods

#### LoadModelFromQueryString

**Signature**:
```csharp
public PackagesViewModel LoadModelFromQueryString(string queryString)
```

**Description**: Parses URL query parameters into a `PackagesViewModel` object.

**Supported Parameters**:
- `TemplateName` - Template to use
- `TemplateVersion` - Specific version or "LTS"
- `ProjectName` - Project name
- `SolutionName` - Solution name
- `CreateSolutionFile` - Boolean flag
- `Packages` - Comma-separated package list
- `UseUnattendedInstall` - Boolean flag
- `DatabaseType` - Database type selection
- `ConnectionString` - Database connection string
- `UserFriendlyName` - Admin user name
- `UserEmail` - Admin email
- `UserPassword` - Admin password
- `IncludeStarterKit` - Boolean flag
- `StarterKitPackage` - Starter kit package name
- `CanIncludeDocker` - Boolean flag
- `IncludeDockerfile` - Boolean flag
- `IncludeDockerCompose` - Boolean flag
- `OnelinerOutput` - Boolean flag
- `RemoveComments` - Boolean flag

**Example URL**:
```
?TemplateName=Umbraco.Templates
&TemplateVersion=14.3.0
&ProjectName=MyProject
&Packages=Umbraco.Community.BlockPreview|1.6.0,Diplo.GodMode|3.0.3
&UseUnattendedInstall=true
&DatabaseType=SQLite
```

**Backward Compatibility**: Handles older URL formats for compatibility with saved links.

---

#### GenerateQueryString

**Signature**:
```csharp
public string GenerateQueryString(PackagesViewModel model, string baseUrl)
```

**Description**: Generates a shareable URL from the current model state.

**Features**:
- URL-encodes all parameters
- Includes only non-default values
- Generates clean, readable URLs
- Supports all model properties

**Returns**: Complete URL with query string

**Example Output**:
```
https://psw.codeshare.co.uk?TemplateName=Umbraco.Templates&TemplateVersion=14.3.0&ProjectName=MyProject&Packages=Umbraco.Community.BlockPreview%7C1.6.0
```

---

## UmbracoVersionService

**File**: `PSW/PSW/Services/UmbracoVersionService.cs`

**Purpose**: Manages Umbraco version lifecycle data and determines LTS/STS status.

### Dependencies

```csharp
public class UmbracoVersionService : IUmbracoVersionService
{
    // No external dependencies - uses PSWConfig injected via methods
}
```

### Public Methods

#### GetLatestLTSVersion

**Signature**:
```csharp
public string GetLatestLTSVersion(PSWConfig config)
```

**Description**: Determines the latest Long-Term Support (LTS) version from configuration.

**Logic**:
1. Reads `UmbracoVersions` array from `PSWConfig`
2. Filters for versions where `IsLTS = true`
3. Finds the version with the latest `ReleaseDate`
4. Returns version string (e.g., "14.3.0")

**Example**:
```csharp
var latestLTS = _umbracoVersionService.GetLatestLTSVersion(_pswConfig);
// Returns: "14.3.0"
```

---

#### GetVersionStatus

**Signature**:
```csharp
public VersionStatus GetVersionStatus(string version, PSWConfig config)
```

**Description**: Determines if a version is active, EOL, or in security-only phase.

**Return Values**:
- `VersionStatus.LTS` - Long-Term Support version
- `VersionStatus.STS` - Short-Term Support version
- `VersionStatus.EOL` - End of Life (no longer supported)
- `VersionStatus.SecurityOnly` - Security updates only

**Logic**:
1. Compares current date with `EndOfLifeDate`
2. Checks `EndOfSecurityDate` if applicable
3. Determines LTS vs STS based on `IsLTS` flag

---

## Service Registration

**File**: `PSW/PSW/Program.cs`

Services are registered in the dependency injection container with **Scoped** lifetime:

```csharp
// Service registration
builder.Services.AddScoped<IScriptGeneratorService, ScriptGeneratorService>();
builder.Services.AddScoped<IPackageService, MarketplacePackageService>();
builder.Services.AddScoped<IQueryStringService, QueryStringService>();
builder.Services.AddScoped<IUmbracoVersionService, UmbracoVersionService>();
```

### Lifetime Explanation

**Scoped Lifetime**: Service instances are created once per request and shared across that request.

**Why Scoped?**
- Services don't maintain state between requests
- Allows for proper disposal of resources
- Efficient for web request handling
- Services can share dependencies within a request

### Additional Infrastructure Services

```csharp
// HTTP Client Factory
builder.Services.AddHttpClient();

// Memory Cache
builder.Services.AddMemoryCache();
```

---

## Service Interfaces

### IScriptGeneratorService

```csharp
public interface IScriptGeneratorService
{
    string GenerateScript(PackagesViewModel model);
    List<string> GenerateUmbracoTemplatesSectionScript(PackagesViewModel model);
    List<string> GenerateCreateSolutionFileScript(PackagesViewModel model);
    List<string> GenerateCreateProjectScript(PackagesViewModel model);
    List<string> GenerateAddProjectToSolutionScript(PackagesViewModel model);
    List<string> GenerateAddStarterKitScript(PackagesViewModel model, bool renderPackageName);
    List<string> GenerateAddPackagesScript(PackagesViewModel model, bool renderPackageName);
    List<string> GenerateRunProjectScript(PackagesViewModel model, bool renderPackageName);
    List<string> GenerateAddDockerComposeScript(PackagesViewModel model);
}
```

### IPackageService

```csharp
public interface IPackageService
{
    Task<PagedPackages> GetAllPackages(
        IMemoryCache memoryCache,
        int cacheDurationMinutes);

    Task<List<string>> GetNugetPackageVersions(
        string packageId,
        bool includePrerelease,
        IMemoryCache memoryCache,
        int cacheDurationMinutes);
}
```

### IQueryStringService

```csharp
public interface IQueryStringService
{
    PackagesViewModel LoadModelFromQueryString(string queryString);
    string GenerateQueryString(PackagesViewModel model, string baseUrl);
}
```

### IUmbracoVersionService

```csharp
public interface IUmbracoVersionService
{
    string GetLatestLTSVersion(PSWConfig config);
    VersionStatus GetVersionStatus(string version, PSWConfig config);
}
```

---

## Testing Services

### Unit Testing Example

```csharp
public class ScriptGeneratorServiceTests
{
    [Fact]
    public void GenerateScript_WithBasicModel_ReturnsValidScript()
    {
        // Arrange
        var config = Options.Create(new PSWConfig());
        var versionService = new Mock<IUmbracoVersionService>();
        var service = new ScriptGeneratorService(config, versionService.Object);

        var model = new PackagesViewModel
        {
            TemplateName = "Umbraco.Templates",
            TemplateVersion = "14.3.0",
            ProjectName = "TestProject"
        };

        // Act
        var result = service.GenerateScript(model);

        // Assert
        Assert.Contains("dotnet new install Umbraco.Templates::14.3.0", result);
        Assert.Contains("dotnet new umbraco", result);
    }
}
```

---

## Performance Considerations

### Caching Strategy

Services leverage `IMemoryCache` for optimal performance:

| Data Type | Cache Duration | Performance Impact |
|-----------|----------------|-------------------|
| All Packages | 60 minutes | 100x improvement |
| Package Versions | 60 minutes | 100x improvement |
| Template Versions | 60 minutes | 50x improvement |

### Optimization Tips

1. **Cache Warming**: Pre-load common packages on startup
2. **Parallel Requests**: Fetch multiple package versions concurrently
3. **Response Compression**: Enable gzip compression for API responses
4. **CDN Integration**: Cache static package data on CDN

---

[← Back to Documentation Index](documentation.md)
