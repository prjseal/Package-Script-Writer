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

### API Testing

#### Using REST Client (VS Code)

1. Install **REST Client** extension
2. Create file: `api-tests.http`

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

#### Browser Testing

- [ ] Chrome (latest)
- [ ] Firefox (latest)
- [ ] Safari (latest)
- [ ] Edge (latest)
- [ ] Mobile Chrome
- [ ] Mobile Safari

---

### Unit Testing (Future)

**Example Test Structure**:

```csharp
using Xunit;
using Moq;

public class ScriptGeneratorServiceTests
{
    [Fact]
    public void GenerateScript_WithBasicModel_ReturnsValidScript()
    {
        // Arrange
        var config = Options.Create(new PSWConfig());
        var versionService = new Mock<IUmbracoVersionService>();
        versionService.Setup(x => x.GetLatestLTSVersion(It.IsAny<PSWConfig>()))
            .Returns("14.3.0");

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
        Assert.Contains("dotnet run", result);
    }

    [Fact]
    public void GenerateScript_WithPackages_IncludesPackageCommands()
    {
        // Test implementation
    }

    [Theory]
    [InlineData("SQLite", "development-database-type SQLite")]
    [InlineData("LocalDb", "development-database-type LocalDB")]
    public void GenerateScript_WithDatabaseType_UsesCorrectSwitch(
            string dbType, string expectedSwitch)
    {
        // Test implementation
    }
}
```

**Run Tests**:
```bash
dotnet test
```

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
