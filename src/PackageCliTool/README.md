# Package Script Writer CLI (`psw`)

An interactive command-line interface for the Package Script Writer API, built with .NET 10.0 and Spectre.Console.

[![NuGet](https://img.shields.io/nuget/v/PackageScriptWriter.Cli.svg)](https://www.nuget.org/packages/PackageScriptWriter.Cli/)
[![Downloads](https://img.shields.io/nuget/dt/PackageScriptWriter.Cli.svg)](https://www.nuget.org/packages/PackageScriptWriter.Cli/)

## Features

- 🎨 **Beautiful CLI Interface** - Built with Spectre.Console for a rich terminal experience
- 🚀 **Dual Mode Operation** - Interactive mode OR command-line flags for automation
- 🎯 **Template Selection** - Choose from Umbraco official templates and community templates with version selection
- 📦 **Package Selection** - Browse and search from 150+ Umbraco Marketplace packages or add custom ones
- 🔢 **Version Selection** - Choose specific versions for each selected package
- ⚡ **Progress Indicators** - Spinners and progress displays during API calls
- 📄 **Script Generation** - Generate complete installation scripts with all options
- 💾 **Export Scripts** - Save generated scripts to files
- ⚙️ **Complete Configuration** - All options from the website's Options tab:
  - Template and project settings
  - Solution file creation
  - Starter kit selection (9 different starter kits)
  - Docker integration (Dockerfile & Docker Compose)
  - Unattended install with database configuration
  - Admin user credentials (with secure password input)
  - Output formatting (one-liner, comment removal)
- 📊 **Configuration Summary** - Review all settings before generating script
- 🔒 **Secure Input** - Password fields are hidden during input
- ✅ **Confirmation Prompts** - Prevent accidental operations
- 🤖 **Automation Ready** - Use CLI flags for CI/CD pipelines and scripts
- 🔄 **Resilient HTTP Client** - Automatic retry logic with exponential backoff using Polly
- 📝 **Advanced Logging** - Comprehensive logging with Serilog (file and console output)
- 🐛 **Verbose Mode** - Enable detailed logging with `--verbose` flag or `PSW_VERBOSE=1` environment variable

## Requirements

- .NET 10.0 SDK or later
- Internet connection (to access the Package Script Writer API)

## Installation

### Option 1: Install as .NET Global Tool (Recommended)

Once published to NuGet, install globally:

**Install Beta Version:**
```bash
dotnet tool install --global PackageScriptWriter.Cli --prerelease
```

**Install Stable Version (when available):**
```bash
dotnet tool install --global PackageScriptWriter.Cli
```

Then run from anywhere:

```bash
psw
```

**Update the tool:**
```bash
# Update to latest beta
dotnet tool update --global PackageScriptWriter.Cli --prerelease

# Update to latest stable
dotnet tool update --global PackageScriptWriter.Cli
```

**Uninstall the tool:**
```bash
dotnet tool uninstall --global PackageScriptWriter.Cli
```

### Option 2: Install from Local Build

1. Navigate to the src directory:
   ```bash
   cd src
   ```

2. Pack the project:
   ```bash
   dotnet pack PackageCliTool -c Release
   ```

3. Install from the local package:
   ```bash
   dotnet tool install --global --add-source ./PackageCliTool/bin/Release PackageScriptWriter.Cli
   ```

4. Run from anywhere:
   ```bash
   psw
   ```

### Option 3: Build and Run from Source

1. Navigate to the src directory:
   ```bash
   cd src
   ```

2. Run the CLI tool:
   ```bash
   dotnet run --project PackageCliTool
   ```

### Option 4: Open in Visual Studio

1. Open `src/PackageCliTool.sln` in Visual Studio
2. Press F5 to build and run

## Usage

The CLI tool supports two modes of operation:
1. **Interactive Mode** - Step-by-step prompts (no flags)
2. **CLI Mode** - Command-line flags for automation

### Command-Line Flags

#### Quick Reference

```bash
# Show help
psw --help
psw -h

# Show version
psw --version
psw -v

# Generate default script
psw --default
psw -d

# Generate custom script with packages (latest versions)
psw -p "uSync,Umbraco.Forms" -n MyProject

# Generate script with specific package versions
psw -p "uSync|17.0.0,clean|7.0.1" -n MyProject

# Full automation example
psw -p "uSync|17.0.0" -n MyProject -s --solution-name MySolution \
    -u --database-type SQLite --admin-email admin@test.com \
    --admin-password "MySecurePass123!" --auto-run
```

#### Available Flags

**General Options:**
- `-h, --help` - Display help information with all available flags
- `-v, --version` - Display the tool version
- `-d, --default` - Generate a default script with minimal configuration

**Script Configuration:**
- `-p, --packages <packages>` - Comma-separated list of packages with optional versions
  - Format: `"Package1|Version1,Package2|Version2"` (e.g., `"uSync|17.0.0,clean|7.0.1"`)
  - Or just package names: `"uSync,Umbraco.Forms"` (automatically uses latest version)
  - Mix both formats: `"uSync|17.0.0,Umbraco.Forms"` (first uses specific version, second uses latest)
- `-t, --template-version <version>` - Template version (Latest, LTS, or specific version like "14.3.0")
- `-n, --project-name <name>` - Project name (default: MyProject)
- `-s, --solution` - Create a solution file
- `--solution-name <name>` - Solution name (used with -s/--solution)

**Starter Kit:**
- `-k, --starter-kit` - Include a starter kit
- `--starter-kit-package <package>` - Starter kit package name (e.g., "clean", "Articulate")

**Docker:**
- `--dockerfile` - Include Dockerfile
- `--docker-compose` - Include Docker Compose file

**Unattended Install:**
- `-u, --unattended` - Use unattended install
- `--database-type <type>` - Database type (SQLite, LocalDb, SQLServer, SQLAzure, SQLCE)
- `--connection-string <string>` - Connection string (for SQLServer/SQLAzure)
- `--admin-name <name>` - Admin user friendly name
- `--admin-email <email>` - Admin email address
- `--admin-password <password>` - Admin password (min 10 characters)

**Output Options:**
- `-o, --oneliner` - Output as one-liner
- `-r, --remove-comments` - Remove comments from script
- `--include-prerelease` - Include prerelease package versions

**Execution:**
- `--auto-run` - Automatically run the generated script
- `--run-dir <directory>` - Directory to run script in

**Debugging:**
- `--verbose` - Enable verbose logging mode (detailed diagnostic output)

#### CLI Examples

**Example 1: Default Script**
```bash
psw --default
```

**Example 2: Simple Custom Script (Latest Versions)**
```bash
psw -p "uSync,Diplo.GodMode" -n MyBlog
```

**Example 3: Script with Specific Package Versions**
```bash
psw -p "uSync|17.0.0,clean|7.0.1" -n MyProject
```

**Example 4: Mixed Package Versions (Some Specific, Some Latest)**
```bash
psw -p "uSync|17.0.0,Umbraco.Forms,Diplo.GodMode|3.0.3" -n MyProject
```

**Example 5: Script with Solution**
```bash
psw -p "uSync|17.0.0" -n MyProject -s --solution-name MySolution
```

**Example 6: Full Automation with Unattended Install**
```bash
psw -p "uSync|17.0.0,Umbraco.Forms|14.2.0" \
    -n MyUmbracoSite \
    -t "14.3.0" \
    -s --solution-name MyUmbracoSolution \
    -u --database-type SQLite \
    --admin-name "Site Administrator" \
    --admin-email "admin@mysite.com" \
    --admin-password "SecurePassword123!"
```

**Example 7: Docker-enabled Setup**
```bash
psw -p "uSync|17.0.0" -n MyDockerProject \
    --dockerfile --docker-compose \
    --auto-run --run-dir ./projects/docker-site
```

**Example 8: One-liner Output**
```bash
psw -p "uSync|17.0.0" -n QuickSite -o -r
```

### Interactive Mode Workflow

1. **Start the CLI**:

   If installed as a global tool:
   ```bash
   psw
   ```

   Or if running from source:
   ```bash
   dotnet run --project PackageCliTool
   ```

2. **Select Packages**:
   - Use arrow keys to navigate through 150+ Umbraco Marketplace packages
   - Press `Space` to select/deselect packages
   - Press `Enter` to confirm selection
   - You can also select "Add custom package..." to enter a package name manually

3. **Select Package Versions**:
   - For each selected package, choose a version from the list
   - The tool fetches versions from the API in real-time

4. **Select Template**:
   - Choose from Umbraco official templates or community templates:
     - Umbraco.Templates (official)
     - Umbraco.Community.Templates.Clean
     - Umbraco.Community.Templates.UmBootstrap

5. **Select Template Version**:
   - Choose "Latest Stable", "Pre-release", or a specific version number
   - The tool fetches available versions from the API

6. **Review Selection**:
   - View your selected packages and versions in a formatted table

7. **Configure Project Options**:
   The CLI will guide you through all configuration options:

   **Template & Project Settings**:
   - Template version (Latest Stable, Latest LTS, or specific version)
   - Project name
   - Create solution file (yes/no)
   - Solution name (if creating solution)

   **Starter Kit Options**:
   - Include starter kit (yes/no)
   - Select starter kit type (clean, Articulate, Portfolio, etc.)

   **Docker Options**:
   - Include Dockerfile (yes/no)
   - Include Docker Compose (yes/no)

   **Unattended Install Options**:
   - Use unattended install (yes/no)
   - Database type (SQLite, LocalDb, SQL Server, SQL Azure, SQLCE)
   - Connection string (for SQL Server/Azure)
   - Admin user friendly name
   - Admin email
   - Admin password (hidden input)

   **Output Format Options**:
   - Output as one-liner (yes/no)
   - Remove comments (yes/no)

8. **Review Configuration Summary**:
   - View all your settings in a formatted table
   - Confirm to proceed or cancel

9. **Generate and Save**:
   - Script is generated using the API with resilient retry logic
   - View the generated script in a formatted panel
   - Optionally save it to a file
   - Optionally auto-run the script

### Example Session

```
 ____            _                           ____ _     ___   _____           _
|  _ \ __ _  ___| | ____ _  __ _  ___       / ___| |   |_ _| |_   _|__   ___ | |
| |_) / _` |/ __| |/ / _` |/ _` |/ _ \     | |   | |    | |    | |/ _ \ / _ \| |
|  __/ (_| | (__|   < (_| | (_| |  __/     | |___| |___ | |    | | (_) | (_) | |
|_|   \__,_|\___|_|\_\__,_|\__, |\___|      \____|_____|___|   |_|\___/ \___/|_|
                           |___/

Package Script Writer - Interactive CLI

Step 1: Select Template

Step 2: Select Template Version

Step 3: Select Packages

Select one or more packages (use Space to select, Enter to confirm):
  [ ] Umbraco.Community.BlockPreview
  [X] Diplo.GodMode
  [X] uSync
  [ ] Umbraco.Community.Contentment
  ...

Step 4: Select Versions

⠋ Fetching versions for Diplo.GodMode...
✓ Selected Diplo.GodMode version 3.0.3
✓ Selected uSync version 12.0.0

Step 5: Final Selection

┌───────────────────────────┬──────────────────┐
│      Package Name         │ Selected Version │
├───────────────────────────┼──────────────────┤
│ Diplo.GodMode             │      3.0.3       │
│ uSync                     │     12.0.0       │
└───────────────────────────┴──────────────────┘

Would you like to generate a complete installation script? (y/n): y

Step 6: Configure Project Options

Template & Project Settings

Select Umbraco template version:
> Latest LTS
  Latest Stable
  14.3.0
  ...

Enter project name [MyProject]: MyBlog

Create a solution file? (y/n): y
Enter solution name [MyBlog]: MyBlog

Starter Kit Options

Include a starter kit? (y/n): n

Docker Options

Include Dockerfile? (y/n): n
Include Docker Compose? (y/n): n

Unattended Install Options

Use unattended install? (y/n): y
Select database type:
> SQLite
  LocalDb
  SQL Server
  ...

Enter admin user friendly name [Administrator]: Site Admin
Enter admin email [admin@example.com]: admin@myblog.com
Enter admin password (min 10 characters): **********

Output Format Options

Output as one-liner? (y/n): n
Remove comments from script? (y/n): n

Configuration Summary

┌─────────────────────┬──────────────────────────┐
│ Setting             │ Value                    │
├─────────────────────┼──────────────────────────┤
│ Template            │ Umbraco.Templates @ LTS  │
│ Project Name        │ MyBlog                   │
│ Solution Name       │ MyBlog                   │
│ Packages            │ 2 package(s) selected    │
│ Unattended Install  │ Enabled                  │
│ Database Type       │ SQLite                   │
│ Admin User          │ Site Admin               │
│ Admin Email         │ admin@myblog.com         │
└─────────────────────┴──────────────────────────┘

Generate script with these settings? (y/n): y

⭐ Generating installation script...

╔═══════════════════════════════════════════╗
║ Generated Installation Script            ║
╠═══════════════════════════════════════════╣
║                                           ║
║ # Install Umbraco templates               ║
║ dotnet new install Umbraco.Templates...   ║
║ ...                                       ║
║                                           ║
╚═══════════════════════════════════════════╝

Would you like to save this script to a file? (y/n): y
Enter file name [install-script.sh]: setup.sh
✓ Script saved to setup.sh

✓ Process completed successfully!
```

## Project Structure

```
src/
├── PackageCliTool.sln          # Solution file
└── PackageCliTool/
    ├── Configuration/          # Application configuration
    │   └── ApiConfiguration.cs
    ├── Exceptions/             # Custom exception types
    │   └── PswException.cs
    ├── Logging/                # Logging setup and error handling
    │   ├── ErrorHandler.cs
    │   └── LoggerSetup.cs
    ├── Models/                 # Data models and DTOs
    │   ├── Api/                # API request/response models
    │   ├── CommandLineOptions.cs
    │   ├── PackageCategory.cs
    │   └── PagedPackagesPackage.cs
    ├── Services/               # Business logic and API communication
    │   ├── ApiClient.cs
    │   ├── PackageSelector.cs
    │   ├── ResilientHttpClient.cs
    │   └── ScriptExecutor.cs
    ├── UI/                     # User interface components
    │   ├── ConfigurationDisplay.cs
    │   ├── ConsoleDisplay.cs
    │   └── InteractivePrompts.cs
    ├── Validation/             # Input validation
    │   └── InputValidator.cs
    ├── Workflows/              # Orchestration logic
    │   ├── CliModeWorkflow.cs
    │   └── InteractiveModeWorkflow.cs
    ├── PackageCliTool.csproj   # Project file (.NET 10.0)
    ├── Program.cs              # Main entry point
    ├── icon.png                # NuGet package icon
    └── README.md               # This file
```

## Code Structure

The CLI tool is built with a clean, modular architecture following separation of concerns:

### 1. **Entry Point & Orchestration**

**Program.cs** - Main application entry point
- Initializes logging (Serilog with file and console output)
- Parses command-line arguments
- Determines execution mode (CLI vs Interactive)
- Orchestrates workflows and services
- Handles global exception handling

### 2. **Workflows** (Orchestration Layer)

**InteractiveModeWorkflow.cs** - Interactive mode orchestration
- Displays welcome banner
- Populates packages from marketplace API
- Guides user through step-by-step prompts
- Generates default or custom scripts
- Handles file saving and script execution

**CliModeWorkflow.cs** - CLI mode orchestration
- Parses command-line options
- Validates input parameters
- Generates scripts from flags
- Executes auto-run if requested

### 3. **Services** (Business Logic Layer)

**ApiClient.cs** - API communication with resilience
- `GetPackageVersionsAsync()` - Fetches package versions from NuGet
- `GenerateScriptAsync()` - Generates installation script via API
- `GetAllPackagesAsync()` - Retrieves all Umbraco Marketplace packages
- Uses ResilientHttpClient for automatic retries

**ResilientHttpClient.cs** - HTTP resilience with Polly
- Automatic retry logic with exponential backoff
- Handles transient failures gracefully
- Configurable retry policies (default: 3 retries)
- Comprehensive logging of retry attempts

**PackageSelector.cs** - Package and template selection logic
- `PopulateAllPackagesAsync()` - Fetches and caches marketplace packages
- `SelectPackagesAsync()` - Multi-select package prompt with search
- `SelectVersionsForPackagesAsync()` - Version selection for each package
- `SelectTemplateAsync()` - Template selection prompt
- `SelectTemplateVersionAsync()` - Template version selection

**ScriptExecutor.cs** - Script execution logic
- Saves scripts to files
- Executes scripts automatically if requested
- Handles cross-platform script execution

### 4. **UI Layer** (User Interface Components)

**ConsoleDisplay.cs** - Static UI displays
- `DisplayWelcomeBanner()` - ASCII art banner
- `DisplayHelp()` - Comprehensive help text
- `DisplayVersion()` - Version information

**ConfigurationDisplay.cs** - Configuration display
- `DisplayFinalSelection()` - Shows selected packages in table
- `DisplayConfigurationSummary()` - Shows full configuration before generation

**InteractivePrompts.cs** - Interactive user prompts
- Project configuration prompts
- Starter kit selection
- Docker options
- Unattended install configuration
- Output format options

### 5. **Models** (Data Transfer Objects)

**CommandLineOptions.cs** - CLI argument parser
- `Parse()` - Parses command-line arguments
- `HasAnyOptions()` - Checks if any options are set
- Properties for all CLI flags (24+ options)

**API Models** (Models/Api/)
- `ScriptModel.cs` - Complete script configuration
- `ScriptRequest/Response.cs` - Script generation DTOs
- `PackageVersionRequest/Response.cs` - Package version DTOs

**Domain Models**
- `PagedPackagesPackage.cs` - Marketplace package model
- `PackageCategory.cs` - Package category enumeration

### 6. **Infrastructure**

**Logging/** - Logging infrastructure
- `LoggerSetup.cs` - Serilog configuration (file + console)
- `ErrorHandler.cs` - Centralized error handling with user-friendly messages

**Configuration/**
- `ApiConfiguration.cs` - API base URL and settings

**Exceptions/**
- `PswException.cs` - Custom exception types with user guidance

**Validation/**
- `InputValidator.cs` - Input validation and sanitization

### Key Features in Code

#### Async/Await Pattern
All API calls use async/await for non-blocking operations:
```csharp
var versions = await apiClient.GetPackageVersionsAsync(package, includePrerelease: false);
```

#### Resilience with Polly
Automatic retry logic with exponential backoff for HTTP requests:
```csharp
var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
    );
```

#### Structured Logging with Serilog
Comprehensive logging to file and console:
```csharp
logger.LogInformation("Fetching package versions for {PackageId}", packageId);
logger.LogDebug("Received response with length {Length}", responseContent.Length);
logger.LogError(ex, "Failed to fetch package versions");
```

Log files are saved to: `logs/psw-cli-{Date}.log`

Enable verbose mode with:
- `--verbose` flag
- `PSW_VERBOSE=1` environment variable

#### Spectre.Console Integration
- **MultiSelectionPrompt** - For selecting multiple packages
- **SelectionPrompt** - For selecting single options (versions, database types, starter kits, templates, etc.)
- **TextPrompt with Secret** - For secure password input
- **Confirm** - For yes/no prompts
- **Status/Spinner** - For showing progress during API calls
- **Table** - For displaying package selections and configuration summary
- **Panel** - For showing generated scripts
- **FigletText** - For ASCII art banner

#### Error Handling
Centralized error handling with custom exceptions and user guidance:
```csharp
try
{
    // API operations
}
catch (ApiException ex)
{
    ErrorHandler.Handle(ex, logger, showStackTrace: verboseMode);
}
```

Custom exceptions include:
- **ApiException** - API communication errors with user-friendly messages
- **PswException** - General CLI errors with guidance

## API Integration

The tool connects to the Package Script Writer API:
- **Base URL**: `https://psw.codeshare.co.uk`
- **Endpoints**:
  - `POST /api/scriptgeneratorapi/getpackageversions` - Get package versions
  - `POST /api/scriptgeneratorapi/generatescript` - Generate installation script
  - `GET /api/scriptgeneratorapi/getallpackages` - Get all Umbraco Marketplace packages

## Configuration

### API Configuration
The API base URL is configured in `Configuration/ApiConfiguration.cs`:
```csharp
public static class ApiConfiguration
{
    public const string ApiBaseUrl = "https://psw.codeshare.co.uk";
}
```

### HTTP Client Settings
Configured in `Services/ApiClient.cs`:
- **Timeout**: 90 seconds (for large package lists)
- **Retry Policy**: 3 retries with exponential backoff (via Polly)
- **Base Address**: From ApiConfiguration

### Logging Configuration
Configured in `Logging/LoggerSetup.cs`:
- **Console Logging**: Always enabled (Info level by default, Debug in verbose mode)
- **File Logging**: Optional (saved to `logs/psw-cli-{Date}.log`)
- **Verbose Mode**: Enable with `--verbose` or `PSW_VERBOSE=1`

## Troubleshooting

### Package Not Found
If a package cannot be found:
- Verify the package name is correct
- Check if the package exists on NuGet.org or Umbraco Marketplace
- Ensure you have internet connectivity
- Enable verbose mode to see detailed API calls: `psw --verbose`

### API Connection Issues
If you cannot connect to the API:
- Check your internet connection
- Verify the API is accessible: https://psw.codeshare.co.uk/api/docs
- Check firewall settings
- The tool automatically retries failed requests 3 times with exponential backoff
- Check log files in `logs/` directory for detailed error information

### Enable Verbose Logging
For detailed diagnostic output:

**Option 1: Command-line flag**
```bash
psw --verbose
```

**Option 2: Environment variable**
```bash
export PSW_VERBOSE=1  # Linux/macOS
set PSW_VERBOSE=1     # Windows CMD
$env:PSW_VERBOSE=1    # Windows PowerShell

psw
```

**Log file location**: `logs/psw-cli-{Date}.log`

Verbose mode shows:
- Detailed API request/response information
- HTTP retry attempts
- Package fetching progress
- Internal workflow steps
- Full exception stack traces

### Slow Package Loading
If package loading is slow:
- The tool fetches 150+ packages from the marketplace on first run
- Subsequent selections use cached data
- Retry logic may add 5-10 seconds for failed requests
- Check your internet connection speed

### .NET Version Issues
Ensure you have .NET 10.0 SDK installed:
```bash
dotnet --version
```

If not, download from: https://dotnet.microsoft.com/download

### Log Files
Log files are automatically created in the `logs/` directory:
- **Format**: `psw-cli-YYYYMMDD.log`
- **Rolling**: One file per day
- **Retention**: Logs are kept indefinitely (manually clean up old logs)
- **Location**: `./logs/` in the current working directory

To view recent logs:
```bash
# Linux/macOS
tail -f logs/psw-cli-$(date +%Y%m%d).log

# Windows PowerShell
Get-Content logs\psw-cli-$(Get-Date -Format 'yyyyMMdd').log -Wait -Tail 50
```

## Development

### Adding New Features

The code is modularized for easy extension:

1. **Add new UI prompts** - Use Spectre.Console prompts
2. **Add new API endpoints** - Extend `ApiClient` class
3. **Customize display** - Modify display methods

### Code Comments

The code includes extensive comments explaining:
- Method purposes
- Parameter descriptions
- Key logic decisions
- API interactions

### Publishing to NuGet

To publish this tool to NuGet.org:

1. **Build and pack the project**:
   ```bash
   cd src
   dotnet pack PackageCliTool -c Release
   ```

2. **Get your NuGet API key** from https://www.nuget.org/account/apikeys

3. **Push to NuGet**:
   ```bash
   dotnet nuget push PackageCliTool/bin/Release/PackageScriptWriter.Cli.1.0.0-beta.nupkg \
     --api-key YOUR_API_KEY \
     --source https://api.nuget.org/v3/index.json
   ```

4. **Verify publication** at https://www.nuget.org/packages/PackageScriptWriter.Cli

#### Version Management

**Beta/Pre-release versions:**
- Format: `1.0.0-beta`, `1.0.0-beta.1`, `1.0.0-rc1`, `1.0.0-alpha`
- Edit `<Version>` in `PackageCliTool.csproj`
- Users must use `--prerelease` flag to install

**Stable versions:**
- Format: `1.0.0`, `1.1.0`, `2.0.0`
- Remove the suffix (e.g., change `1.0.0-beta` to `1.0.0`)
- Users can install without `--prerelease` flag

**Version progression example:**
1. `1.0.0-alpha` - Early testing
2. `1.0.0-beta` - Feature complete, testing
3. `1.0.0-beta.2` - Beta updates
4. `1.0.0-rc1` - Release candidate
5. `1.0.0` - Stable release
6. `1.0.1` - Patch release
7. `1.1.0` - Minor version with new features
8. `2.0.0` - Major version with breaking changes

## Dependencies

- **Spectre.Console** (v0.49.1) - Terminal UI framework
  - Rich formatting and colors
  - Interactive prompts
  - Progress indicators
  - Tables and panels

- **Serilog** (v4.2.0) - Structured logging framework
  - `Serilog.Extensions.Logging` (v8.0.0) - Microsoft.Extensions.Logging integration
  - `Serilog.Sinks.Console` (v6.0.0) - Console output
  - `Serilog.Sinks.File` (v6.0.0) - File output with rolling logs

- **Polly** (v8.5.0) - Resilience and transient fault handling
  - `Polly.Extensions.Http` (v3.0.0) - HTTP retry policies

- **Microsoft.Extensions.Logging** (v9.0.0) - Logging abstractions
  - `Microsoft.Extensions.Logging.Console` (v9.0.0) - Console logging

- **Microsoft.SourceLink.GitHub** (v8.0.0) - Source debugging support

## License

This project is part of the Package Script Writer application.

## Links

- **Main Repository**: https://github.com/prjseal/Package-Script-Writer
- **API Documentation**: https://github.com/prjseal/Package-Script-Writer/blob/main/.github/api-reference.md
- **Swagger UI**: https://psw.codeshare.co.uk/api/docs

## Support

For issues or questions:
1. Check the API documentation
2. Review the Swagger UI for endpoint details
3. Open an issue on the main repository
