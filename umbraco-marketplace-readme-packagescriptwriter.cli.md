# Package Script Writer CLI (`psw`)

An interactive command-line tool for generating Umbraco installation scripts, built with .NET 10.0 and Spectre.Console.

[![NuGet](https://img.shields.io/nuget/v/PackageScriptWriter.Cli.svg)](https://www.nuget.org/packages/PackageScriptWriter.Cli/)
[![Downloads](https://img.shields.io/nuget/dt/PackageScriptWriter.Cli.svg)](https://www.nuget.org/packages/PackageScriptWriter.Cli/)

## Features

- 🎨 **Beautiful CLI Interface** - Built with Spectre.Console for a rich terminal experience
- 🚀 **Dual Mode Operation** - Interactive mode OR command-line flags for automation
- 🎯 **Template Selection** - Choose from Umbraco official templates and community templates with version selection
- 📦 **Package Selection** - Browse and search from 150+ Umbraco Marketplace packages or add custom ones
- 🔢 **Version Selection** - Choose specific versions for each selected package
- ⚡ **Progress Indicators** - Spinners and progress displays during data fetching
- 📄 **Script Generation** - Generate complete installation scripts locally with all options
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
- Internet connection (to fetch package information from the Umbraco Marketplace API)

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

# Display Umbraco versions table
psw versions

# Generate default script
psw --default
psw -d

# Generate custom script with packages (latest versions)
psw -p "uSync,Umbraco.Forms" -n MyProject

# Generate script with specific package versions
psw -p "uSync|17.0.0,clean|7.0.1" -n MyProject

# Use custom template package
psw --template-package Umbraco.Community.Templates.Clean -t 14.3.0 -n MyProject

# Full automation example
psw -p "uSync|17.0.0" -n MyProject -s MySolution \
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
- `--template-package <name>` - Template package name (optional, skips template install if not specified)
  - Examples: "Umbraco.Templates", "Umbraco.Community.Templates.Clean"
  - If omitted, no template installation command will be generated
- `-t, --template-package <templatePackage>` - Template package name with optional version
  - Format: `"PackageName|Version1"` (e.g., `"Umbraco.Templates|17.0.2"`)
  - Or just package names: `"uSync,Umbraco.Forms"` (automatically uses latest version)
- `-n, --project-name <name>` - Project name (default: MyProject)
- `-s, --solution` <name>` - Solution name

**Starter Kit:**

- `-k, --starter-kit <package>` - Starter kit package name (e.g., "clean", "Articulate") with optional version
  - Format: `"Package|Version"` (e.g., `"clean|7.0.3"`)
  - Or just package name: `"clean"` (automatically uses latest version)

**Docker:**

- `--dockerfile` - Include Dockerfile
- `--docker-compose` - Include Docker Compose file

**Unattended Install:**

- `-u, --unattended-defaults` - Use unattended install default values
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
