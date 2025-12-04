# Configuration

Complete guide to configuring the Package Script Writer application.

## Table of Contents
- [Application Settings](#application-settings)
- [Dependency Injection](#dependency-injection)
- [Middleware Configuration](#middleware-configuration)
- [Caching Configuration](#caching-configuration)
- [Environment-Specific Settings](#environment-specific-settings)
- [Logging Configuration](#logging-configuration)

---

## Application Settings

**File**: `appsettings.json`

### Complete Configuration

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "PSW": {
    "CacheDurationMinutes": 60,
    "UmbracoVersions": [
      {
        "Version": "14",
        "ReleaseDate": "2024-05-30",
        "EndOfLifeDate": "2027-12-30",
        "EndOfSecurityDate": "2027-12-30",
        "IsLTS": true
      },
      {
        "Version": "13",
        "ReleaseDate": "2023-12-14",
        "EndOfLifeDate": "2024-12-14",
        "IsLTS": false
      },
      {
        "Version": "12",
        "ReleaseDate": "2023-06-29",
        "EndOfLifeDate": "2024-06-29",
        "IsLTS": false
      },
      {
        "Version": "11",
        "ReleaseDate": "2022-12-08",
        "EndOfLifeDate": "2023-12-08",
        "IsLTS": false
      },
      {
        "Version": "10",
        "ReleaseDate": "2022-06-16",
        "EndOfLifeDate": "2025-12-16",
        "EndOfSecurityDate": "2025-12-16",
        "IsLTS": true
      },
      {
        "Version": "9",
        "ReleaseDate": "2021-09-28",
        "EndOfLifeDate": "2023-09-28",
        "IsLTS": false
      },
      {
        "Version": "8",
        "ReleaseDate": "2019-02-26",
        "EndOfLifeDate": "2023-02-26",
        "IsLTS": false
      },
      {
        "Version": "7",
        "ReleaseDate": "2013-11-19",
        "EndOfLifeDate": "2023-09-30",
        "IsLTS": false
      }
    ]
  }
}
```

---

## PSW Configuration Section

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `CacheDurationMinutes` | int | 60 | Cache expiration time in minutes |
| `UmbracoVersions` | Array | See above | Umbraco version lifecycle data |

### Cache Duration

Controls how long data is cached in memory:
- **Package List**: 60 minutes
- **Package Versions**: 60 minutes
- **Template Versions**: 60 minutes

**Tuning Recommendations**:
- **Development**: 5-10 minutes (faster cache refresh)
- **Production**: 60-120 minutes (reduce API calls)
- **High Traffic**: 120+ minutes (minimize external API load)

---

### Umbraco Version Configuration

Each version object contains:

```json
{
  "Version": "14",
  "ReleaseDate": "2024-05-30",
  "EndOfLifeDate": "2027-12-30",
  "EndOfSecurityDate": "2027-12-30",
  "IsLTS": true
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Version` | string | Yes | Major version number |
| `ReleaseDate` | string (ISO 8601) | Yes | Official release date |
| `EndOfLifeDate` | string (ISO 8601) | No | End of life date |
| `EndOfSecurityDate` | string (ISO 8601) | No | End of security updates |
| `IsLTS` | boolean | Yes | Long-term support flag |

**Version Status Calculation**:
- **Active LTS**: `IsLTS = true` and current date < `EndOfLifeDate`
- **Active STS**: `IsLTS = false` and current date < `EndOfLifeDate`
- **Security Only**: Current date between `EndOfLifeDate` and `EndOfSecurityDate`
- **EOL**: Current date > `EndOfLifeDate` (or `EndOfSecurityDate` if present)

---

## Dependency Injection

**File**: `Program.cs`

### Service Registration

```csharp
var builder = WebApplication.CreateBuilder(args);

// MVC Services
builder.Services.AddControllersWithViews()
    .AddRazorOptions(options => options.ViewLocationFormats.Add("/{0}.cshtml"));

// Infrastructure Services
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();

// Application Services (Scoped)
builder.Services.AddScoped<IScriptGeneratorService, ScriptGeneratorService>();
builder.Services.AddScoped<IPackageService, MarketplacePackageService>();
builder.Services.AddScoped<IQueryStringService, QueryStringService>();
builder.Services.AddScoped<IUmbracoVersionService, UmbracoVersionService>();

// Configuration Binding
builder.Services.Configure<PSWConfig>(
    builder.Configuration.GetSection(PSWConfig.SectionName)
);

var app = builder.Build();
```

### Service Lifetimes

| Service | Lifetime | Reason |
|---------|----------|--------|
| Controllers | Scoped | Per-request |
| ScriptGeneratorService | Scoped | No state, per-request |
| MarketplacePackageService | Scoped | Uses HttpClient per request |
| QueryStringService | Scoped | No state, per-request |
| UmbracoVersionService | Scoped | No state, per-request |
| HttpClient | Scoped | Managed by HttpClientFactory |
| IMemoryCache | Singleton | Shared across requests |

### HttpClientFactory

```csharp
builder.Services.AddHttpClient();
```

**Benefits**:
- Proper disposal of HttpClient instances
- Connection pooling
- DNS refresh handling
- Named/typed clients support

**Usage in Services**:
```csharp
public class MarketplacePackageService : IPackageService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public MarketplacePackageService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<PagedPackages> GetAllPackages()
    {
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync("https://marketplace.umbraco.com/...");
        // ...
    }
}
```

---

## Middleware Configuration

**File**: `Program.cs`

### Middleware Pipeline

```csharp
var app = builder.Build();

// Environment-specific middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // HTTP Strict Transport Security
}

// HTTPS Redirection
app.UseHttpsRedirection();

// Custom Security Headers
app.UseMiddleware<SecurityHeadersMiddleware>();

// Static Files
app.UseStaticFiles();

// Routing
app.UseRouting();

// Authorization (no authentication configured)
app.UseAuthorization();

// MVC Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

### Middleware Order

Order matters! The pipeline executes in the order defined:

1. **Exception Handler** (Production only)
2. **HSTS** (Production only)
3. **HTTPS Redirection** (Always)
4. **Security Headers** (Custom middleware)
5. **Static Files** (CSS, JS, images)
6. **Routing** (Match endpoints)
7. **Authorization** (Check permissions)
8. **Endpoint Execution** (Controllers)

---

### HSTS Configuration

**HTTP Strict Transport Security** forces HTTPS connections.

**Default Configuration**:
```csharp
app.UseHsts();
```

**Custom Configuration** (optional):
```csharp
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
    options.Preload = true;
});
```

**Headers Added**:
```
Strict-Transport-Security: max-age=31536000; includeSubDomains
```

---

### Static Files Configuration

```csharp
app.UseStaticFiles();
```

**Default Settings**:
- **Directory**: `wwwroot/`
- **Caching**: Browser caching enabled
- **File Types**: All common web file types

**Custom Configuration** (optional):
```csharp
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Set cache headers for static assets
        ctx.Context.Response.Headers.Append(
            "Cache-Control", "public,max-age=31536000");
    }
});
```

---

## Caching Configuration

### In-Memory Cache

**Registration**:
```csharp
builder.Services.AddMemoryCache();
```

**Configuration Options**:
```csharp
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024; // Maximum cache size (entries)
    options.CompactionPercentage = 0.2; // Compact 20% when limit reached
    options.ExpirationScanFrequency = TimeSpan.FromMinutes(5); // Scan for expired entries
});
```

### Cache Entry Options

**Usage in Services**:
```csharp
var cacheEntryOptions = new MemoryCacheEntryOptions()
    .SetAbsoluteExpiration(TimeSpan.FromMinutes(cacheDurationMinutes))
    .SetPriority(CacheItemPriority.Normal);

memoryCache.Set(cacheKey, data, cacheEntryOptions);
```

**Cache Priorities**:
- `CacheItemPriority.Low` - Evicted first
- `CacheItemPriority.Normal` - Default
- `CacheItemPriority.High` - Evicted last
- `CacheItemPriority.NeverRemove` - Never evicted

### Cache Keys

| Key Pattern | Data | TTL |
|-------------|------|-----|
| `all-packages` | Umbraco Marketplace packages | 60 min |
| `package-versions-{packageId}` | NuGet package versions | 60 min |
| `umbraco-templates` | Umbraco template versions | 60 min |

---

## Environment-Specific Settings

### Development Settings

**File**: `appsettings.Development.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "System": "Information",
      "Microsoft": "Information",
      "Microsoft.AspNetCore": "Debug"
    }
  },
  "DetailedErrors": true,
  "PSW": {
    "CacheDurationMinutes": 5
  }
}
```

**Key Differences**:
- More verbose logging (`Debug` level)
- Detailed error pages enabled
- Shorter cache duration for faster iteration

---

### Production Settings

**File**: `appsettings.Production.json` (not committed to repo)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "PSW": {
    "CacheDurationMinutes": 120
  }
}
```

**Key Differences**:
- Minimal logging (`Warning` level only)
- Longer cache duration
- No detailed errors exposed

---

### Environment Detection

```csharp
if (app.Environment.IsDevelopment())
{
    // Development-only middleware
}
else if (app.Environment.IsProduction())
{
    // Production-only middleware
}
else if (app.Environment.IsStaging())
{
    // Staging-specific configuration
}
```

**Environment Variable**:
```bash
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_ENVIRONMENT=Staging
```

---

## Logging Configuration

### Default Logging

**Providers**:
- Console (enabled in Development)
- Debug (enabled in Development)
- EventSource

### Log Levels

| Level | Description | Example |
|-------|-------------|---------|
| `Trace` | Most detailed | Method entry/exit |
| `Debug` | Debugging info | Variable values |
| `Information` | Informational | Request processing |
| `Warning` | Warnings | Deprecated API usage |
| `Error` | Errors | Unhandled exceptions |
| `Critical` | Critical failures | Database connection lost |

### Custom Logging Example

```csharp
public class ScriptGeneratorService : IScriptGeneratorService
{
    private readonly ILogger<ScriptGeneratorService> _logger;

    public ScriptGeneratorService(ILogger<ScriptGeneratorService> logger)
    {
        _logger = logger;
    }

    public string GenerateScript(PackagesViewModel model)
    {
        _logger.LogInformation("Generating script for project: {ProjectName}", model.ProjectName);

        try
        {
            // ... generation logic
            _logger.LogDebug("Script generated successfully");
            return script;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating script");
            throw;
        }
    }
}
```

### Structured Logging

Use structured logging with named parameters:

```csharp
// Good
_logger.LogInformation("User {UserId} generated script for {ProjectName}", userId, projectName);

// Avoid
_logger.LogInformation($"User {userId} generated script for {projectName}");
```

**Benefits**:
- Better searchability in log aggregation tools
- Easier querying
- More context

---

## MVC Configuration

### Razor Options

```csharp
builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        // Add custom view location formats
        options.ViewLocationFormats.Add("/{0}.cshtml");
    });
```

**Default View Locations**:
1. `/Views/{ControllerName}/{ViewName}.cshtml`
2. `/Views/Shared/{ViewName}.cshtml`

**With Custom Format**:
3. `/{ViewName}.cshtml` (root-level views)

### JSON Options

```csharp
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
```

---

## Route Configuration

### Default Route

```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

**Pattern Explanation**:
- `{controller=Home}` - Controller name (default: Home)
- `{action=Index}` - Action name (default: Index)
- `{id?}` - Optional ID parameter

**Examples**:
- `/` → `HomeController.Index()`
- `/Home/Index` → `HomeController.Index()`
- `/Home/Error/123` → `HomeController.Error(123)`

### API Route

Controllers use attribute routing:

```csharp
[Route("api/[controller]")]
[ApiController]
public class ScriptGeneratorApiController : ControllerBase
{
    [HttpPost("generatescript")]
    public IActionResult GenerateScript([FromBody] GeneratorApiRequest request)
    {
        // ...
    }
}
```

**Generated Routes**:
- `POST /api/scriptgeneratorapi/generatescript`
- `POST /api/scriptgeneratorapi/getpackageversions`
- `GET /api/scriptgeneratorapi/clearcache`

---

## Configuration Best Practices

### 1. Use Options Pattern

```csharp
// Register
builder.Services.Configure<PSWConfig>(
    builder.Configuration.GetSection(PSWConfig.SectionName)
);

// Inject
public class MyService
{
    private readonly PSWConfig _config;

    public MyService(IOptions<PSWConfig> config)
    {
        _config = config.Value;
    }
}
```

### 2. Never Commit Secrets

- Use **User Secrets** for development
- Use **Environment Variables** for production
- Use **Azure Key Vault** for sensitive production secrets

### 3. Environment-Specific Settings

Use separate `appsettings.{Environment}.json` files for each environment.

### 4. Validate Configuration on Startup

```csharp
var config = builder.Configuration.GetSection("PSW").Get<PSWConfig>();
if (config.CacheDurationMinutes < 1)
{
    throw new InvalidOperationException("Cache duration must be at least 1 minute");
}
```

---

[← Back to Documentation Index](documentation.md)
