# Package Script Writer CLI (`psw`)

An interactive command-line interface for the Package Script Writer API, built with .NET 10.0 and Spectre.Console.

[![NuGet](https://img.shields.io/nuget/v/PackageScriptWriter.Cli.svg)](https://www.nuget.org/packages/PackageScriptWriter.Cli/)
[![Downloads](https://img.shields.io/nuget/dt/PackageScriptWriter.Cli.svg)](https://www.nuget.org/packages/PackageScriptWriter.Cli/)

## Features

- 🎨 **Beautiful CLI Interface** - Built with Spectre.Console for a rich terminal experience
- 🚀 **Dual Mode Operation** - Interactive mode OR command-line flags for automation
- 📦 **Package Selection** - Multi-select from popular Umbraco packages or add custom ones
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
- `-n, --project-name <name>` - Project name (default: MyUmbracoProject)
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
   - Use arrow keys to navigate
   - Press `Space` to select/deselect packages
   - Press `Enter` to confirm selection
   - You can also select "Add custom package..." to enter a package name manually

3. **Select Versions**:
   - For each selected package, choose a version from the list
   - The tool fetches versions from the API in real-time

4. **Review Selection**:
   - View your selected packages and versions in a formatted table

5. **Configure Project Options**:
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

6. **Review Configuration Summary**:
   - View all your settings in a formatted table
   - Confirm to proceed or cancel

7. **Generate and Save**:
   - Script is generated using the API
   - View the generated script in a formatted panel
   - Optionally save it to a file

### Example Session

```
 ____            _                           ____ _     ___   _____           _
|  _ \ __ _  ___| | ____ _  __ _  ___       / ___| |   |_ _| |_   _|__   ___ | |
| |_) / _` |/ __| |/ / _` |/ _` |/ _ \     | |   | |    | |    | |/ _ \ / _ \| |
|  __/ (_| | (__|   < (_| | (_| |  __/     | |___| |___ | |    | | (_) | (_) | |
|_|   \__,_|\___|_|\_\__,_|\__, |\___|      \____|_____|___|   |_|\___/ \___/|_|
                           |___/

Package Script Writer - Interactive CLI

Step 1: Select Packages

Select one or more packages (use Space to select, Enter to confirm):
  [ ] Umbraco.Community.BlockPreview
  [X] Diplo.GodMode
  [X] uSync
  [ ] Umbraco.Community.Contentment
  ...

Step 2: Select Versions

⠋ Fetching versions for Diplo.GodMode...
✓ Selected Diplo.GodMode version 3.0.3
✓ Selected uSync version 12.0.0

Step 3: Final Selection

┌───────────────────────────┬──────────────────┐
│      Package Name         │ Selected Version │
├───────────────────────────┼──────────────────┤
│ Diplo.GodMode             │      3.0.3       │
│ uSync                     │     12.0.0       │
└───────────────────────────┴──────────────────┘

Would you like to generate a complete installation script? (y/n): y

Step 4: Configure Project Options

Template & Project Settings

Select Umbraco template version:
> Latest LTS
  Latest Stable
  14.3.0
  ...

Enter project name [MyUmbracoProject]: MyBlog

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
    ├── PackageCliTool.csproj   # Project file (.NET 10.0)
    ├── Program.cs              # Main application code
    └── README.md               # This file
```

## Code Structure

### Main Components

1. **Program Class** - Entry point and orchestration
   - `Main()` - Application entry point with CLI argument parsing
   - `RunCLIModeAsync()` - Executes tool in CLI mode with flags
   - `RunInteractiveModeAsync()` - Executes tool in interactive mode
   - `DisplayHelp()` - Shows comprehensive help with all flags
   - `DisplayVersion()` - Shows version information
   - `DisplayWelcomeBanner()` - Shows ASCII art banner
   - `GenerateCustomScriptFromOptionsAsync()` - Generates script from CLI options
   - `SelectPackagesAsync()` - Multi-select package prompt
   - `SelectVersionsForPackagesAsync()` - Version selection for each package
   - `DisplayFinalSelection()` - Shows selected packages in a table
   - `GenerateAndDisplayScriptAsync()` - Comprehensive configuration wizard for all options
   - `DisplayConfigurationSummary()` - Shows configuration summary table before generation

2. **CommandLineOptions Class** - CLI argument parser
   - `Parse()` - Parses command-line arguments into options
   - `HasAnyOptions()` - Checks if any configuration options are set
   - Properties for all CLI flags (22+ options)

3. **ApiClient Class** - API communication
   - `GetPackageVersionsAsync()` - Fetches package versions from NuGet
   - `GenerateScriptAsync()` - Generates installation script via API

4. **Data Models** - Request/Response DTOs
   - `PackageVersionRequest/Response` - Package version lookup
   - `ScriptRequest/Response` - Script generation
   - `ScriptModel` - Complete configuration with all 17 options

### Key Features in Code

#### Async/Await Pattern
All API calls use async/await for non-blocking operations:
```csharp
var versions = await apiClient.GetPackageVersionsAsync(package, includePrerelease: false);
```

#### Spectre.Console Integration
- **MultiSelectionPrompt** - For selecting multiple packages
- **SelectionPrompt** - For selecting single options (versions, database types, starter kits, etc.)
- **TextPrompt with Secret** - For secure password input
- **Confirm** - For yes/no prompts (checkboxes equivalent)
- **Status/Spinner** - For showing progress during API calls
- **Table** - For displaying package selections and configuration summary
- **Panel** - For showing generated scripts
- **FigletText** - For ASCII art banner

#### Error Handling
Comprehensive try-catch blocks with user-friendly error messages:
```csharp
try
{
    // API operations
}
catch (Exception ex)
{
    AnsiConsole.MarkupLine($"[red]✗ Error: {ex.Message}[/]");
}
```

## API Integration

The tool connects to the Package Script Writer API:
- **Base URL**: `https://psw.codeshare.co.uk`
- **Endpoints**:
  - `POST /api/scriptgeneratorapi/getpackageversions` - Get package versions
  - `POST /api/scriptgeneratorapi/generatescript` - Generate installation script

## Configuration

### Popular Packages List
The CLI includes a predefined list of popular Umbraco packages in `Program.cs`:

```csharp
private static readonly List<string> PopularPackages = new()
{
    "Umbraco.Community.BlockPreview",
    "Diplo.GodMode",
    "uSync",
    "Umbraco.Community.Contentment",
    // ... more packages
};
```

You can customize this list to include your preferred packages.

### API Timeout
Default timeout is 30 seconds. Modify in `ApiClient` constructor:
```csharp
_httpClient = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(30)
};
```

## Troubleshooting

### Package Not Found
If a package cannot be found:
- Verify the package name is correct
- Check if the package exists on NuGet.org
- Ensure you have internet connectivity

### API Connection Issues
If you cannot connect to the API:
- Check your internet connection
- Verify the API is accessible: https://psw.codeshare.co.uk/api/docs
- Check firewall settings

### .NET Version Issues
Ensure you have .NET 10.0 SDK installed:
```bash
dotnet --version
```

If not, download from: https://dotnet.microsoft.com/download

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
