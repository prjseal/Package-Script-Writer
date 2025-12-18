# Package Script Writer CLI Documentation

<div align="center">

[![NuGet](https://img.shields.io/nuget/v/PackageScriptWriter.Cli.svg)](https://www.nuget.org/packages/PackageScriptWriter.Cli/)
[![Downloads](https://img.shields.io/nuget/dt/PackageScriptWriter.Cli.svg)](https://www.nuget.org/packages/PackageScriptWriter.Cli/)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)

**Interactive command-line tool for generating Umbraco CMS installation scripts**

[Quick Start](#-quick-start) · [Features](#-features) · [Installation](#-installation) · [Usage](#-usage) · [Contributing](#-contributing)

</div>

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Quick Start](#-quick-start)
- [Features](#-features)
- [Installation](#-installation)
  - [Install from NuGet](#install-from-nuget-recommended)
  - [Build from Source](#build-from-source)
- [Usage](#-usage)
  - [Interactive Mode](#interactive-mode)
  - [CLI Mode](#cli-mode)
  - [Command Reference](#command-reference)
- [Documentation](#-documentation)
- [Contributing](#-contributing)
- [Reporting Issues](#-reporting-issues)
- [Support & Community](#-support--community)
- [License](#-license)

---

## 🎯 Overview

The **Package Script Writer CLI** (`psw`) is a powerful command-line tool that brings the full functionality of the Package Script Writer to your terminal. Built with .NET 10.0 and Spectre.Console, it provides a beautiful, interactive experience for generating Umbraco CMS installation scripts.

### Why Use the CLI?

- **🚀 Speed** - Generate scripts faster than using the web interface
- **🤖 Automation** - Perfect for CI/CD pipelines and automated workflows
- **💾 Templates** - Save and reuse configurations across projects
- **📊 History** - Track all generated scripts automatically
- **🔒 Security** - Built-in command validation and secure input handling
- **🎨 Beautiful UI** - Rich terminal interface with colors, tables, and progress indicators

### Perfect For

- Developers who prefer the command line
- CI/CD automation and scripting
- Team standardization with reusable templates
- Quick project setup workflows
- Batch script generation

---

## 🚀 Quick Start

Get up and running in seconds:

```bash
# Install the tool globally from NuGet
dotnet tool install --global PackageScriptWriter.Cli

# Launch interactive mode
psw

# Or generate a default script immediately
psw --default

# Or automate with specific packages
psw -p "uSync,Diplo.GodMode" -n MyProject
```

That's it! You're ready to generate Umbraco installation scripts.

---

## ✨ Features

### 🎨 Dual Mode Operation

**Interactive Mode** - Beautiful terminal UI with step-by-step prompts:
- Browse and search 500+ Umbraco Marketplace packages
- Multi-select package picker with search functionality
- Version selection for templates and packages
- Complete configuration workflow with validation
- Live progress indicators during API calls
- Configuration summary before generation

**CLI Mode** - Full automation with command-line flags:
- All configuration options available as flags
- Perfect for scripts and CI/CD pipelines
- One-liner script generation
- Auto-execution capabilities
- Batch processing support

### 📦 Script Generation

Generate complete, production-ready installation scripts:
- **Template Installation** - Umbraco official and community templates
- **Project Creation** - With solution file support
- **Package Management** - Install with specific version pinning
- **Starter Kits** - Choose from 9 different starter kits
- **Docker Support** - Generate Dockerfile and Docker Compose files
- **Unattended Install** - Pre-configure database and admin credentials
- **Database Setup** - Support for SQLite, LocalDb, SQL Server, SQL Azure
- **Output Formats** - Multi-line or one-liner with comment removal

### 💾 Template System

Save time with reusable configuration templates:
- **Save** - Create templates from any configuration
- **Load** - Quickly apply saved templates with optional overrides
- **Export/Import** - Share templates with your team via YAML files
- **Validation** - Built-in template validation before import
- **Organization** - Tag and describe templates for easy discovery

**[📖 Read the Template Guide](cli/templates.md)**

### 📊 History Tracking

Automatic tracking of all generated scripts:
- **View History** - See all previously generated scripts
- **Re-run** - Regenerate and execute previous scripts
- **Execution Tracking** - Track which scripts were executed and their status
- **Audit Trail** - Complete record of all script generations
- **Search & Filter** - Find scripts by project name, template, or date

**[📖 Read the History Guide](cli/history.md)**

### 🔒 Security Features

Built-in security to protect your systems:
- **Command Allowlist** - Only approved dotnet commands can be executed
- **Validation** - All scripts validated before execution
- **Secure Input** - Password fields are hidden during input
- **No Script Storage** - Scripts regenerated from config (prevents tampering)
- **Safe Defaults** - Secure configurations by default

**[📖 Read the Security Documentation](cli/security.md)**

### 🔄 Resilience & Logging

Enterprise-grade reliability:
- **Automatic Retries** - Exponential backoff with Polly for transient failures
- **Comprehensive Logging** - File and console logging with Serilog
- **Verbose Mode** - Detailed diagnostics with `--verbose` flag
- **Error Handling** - User-friendly error messages with guidance
- **API Resilience** - Graceful handling of API timeouts and failures

---

## 💻 Installation

### Install from NuGet (Recommended)

The easiest way to install the CLI tool is from NuGet as a global .NET tool:

```bash
# Install globally
dotnet tool install --global PackageScriptWriter.Cli

# Verify installation
psw --version
```

**Update to the latest version:**
```bash
dotnet tool update --global PackageScriptWriter.Cli
```

**Uninstall:**
```bash
dotnet tool uninstall --global PackageScriptWriter.Cli
```

### Requirements

- **.NET 10.0 SDK** or later ([Download](https://dotnet.microsoft.com/download))
- **Internet connection** (to fetch package information from Umbraco Marketplace)

### Build from Source

For development or contributing:

1. **Clone the repository:**
   ```bash
   git clone https://github.com/prjseal/Package-Script-Writer.git
   cd Package-Script-Writer
   ```

2. **Build and pack:**
   ```bash
   cd src
   dotnet pack PackageCliTool -c Release
   ```

3. **Install from local package:**
   ```bash
   dotnet tool install --global --add-source ./PackageCliTool/bin/Release PackageScriptWriter.Cli
   ```

4. **Run:**
   ```bash
   psw
   ```

**[📖 Full Installation Guide](../src/PackageCliTool/README.md#installation)**

---

## 📖 Usage

The CLI tool supports two modes of operation: **Interactive Mode** and **CLI Mode**.

### Interactive Mode

Launch the interactive terminal UI with step-by-step prompts:

```bash
psw
```

**Workflow:**
1. **Template Selection** - Choose Umbraco template and version
2. **Package Selection** - Browse and select packages (multi-select with Space)
3. **Version Selection** - Choose specific versions for each package
4. **Configuration** - Set project name, solution, starter kit, Docker, etc.
5. **Unattended Install** - Configure database and admin credentials
6. **Review** - See configuration summary
7. **Generate** - Create the installation script
8. **Save/Execute** - Optionally save to file or execute immediately

### CLI Mode

Use command-line flags for automation and scripting:

```bash
# Generate default script
psw --default

# Custom script with packages (latest versions)
psw -p "uSync,Diplo.GodMode" -n MyProject

# Specific package versions
psw -p "uSync|17.0.0,clean|7.0.1" -n MyProject

# Full automation with unattended install
psw -p "uSync|17.0.0" -n MyProject -s MySolution \
    -u --database-type SQLite \
    --admin-email admin@test.com \
    --admin-password "SecurePass123!" \
    --auto-run
```

### Command Reference

#### General Commands

```bash
psw --help              # Show help information
psw --version           # Show version number
psw versions            # Display Umbraco versions table
psw --default           # Generate default script
```

#### Script Configuration Flags

```bash
-p, --packages          # Packages: "pkg1|ver1,pkg2|ver2"
-t, --template-package  # Template package name and version
-n, --project-name      # Project name (default: MyProject)
-s, --solution          # Solution name
-k, --starter-kit       # Starter kit package name
```

#### Docker Options

```bash
--dockerfile            # Include Dockerfile
--docker-compose        # Include Docker Compose file
```

#### Unattended Install

```bash
-u, --unattended-defaults   # Use unattended install
--database-type             # SQLite, LocalDb, SQLServer, etc.
--connection-string         # Database connection string
--admin-name                # Admin user name
--admin-email               # Admin email address
--admin-password            # Admin password (min 10 chars)
```

#### Output Options

```bash
-o, --oneliner          # Output as one-liner
-r, --remove-comments   # Remove comments from script
--include-prerelease    # Include prerelease package versions
--verbose               # Enable verbose logging
```

#### Execution

```bash
--auto-run              # Automatically run the generated script
--run-dir               # Directory to run script in
```

#### Template Commands

```bash
psw template save <name> [options]      # Save configuration as template
psw template load <name> [options]      # Load and execute template
psw template list                       # List all templates
psw template delete <name>              # Delete template
psw template export <name>              # Export template to YAML
psw template import <file>              # Import template from YAML
psw template validate <file>            # Validate template file
```

#### History Commands

```bash
psw history list                        # List recent script generations
psw history show <#>                    # Show details of specific entry
psw history rerun <#>                   # Re-run a previous script
psw history delete <#>                  # Delete history entry
psw history clear                       # Clear all history
```

**[📖 Complete Usage Guide](../src/PackageCliTool/README.md#usage)**

---

## 📚 Documentation

Comprehensive documentation is available for all aspects of the CLI tool:

### Core Documentation

| Document | Description |
|----------|-------------|
| **[README](../src/PackageCliTool/README.md)** | Complete CLI tool documentation with all features and options |
| **[Release Notes](cli/release-notes.md)** | Version history and changelog |
| **[History System](cli/history.md)** | History feature documentation (`psw history` commands) |
| **[Templates Guide](cli/templates.md)** | Template system documentation with examples |
| **[Security Guide](cli/security.md)** | Security features and command allowlist documentation |

### Related Documentation

| Document | Description |
|----------|-------------|
| [API Reference](api-reference.md) | REST API endpoints used by the CLI |
| [Architecture](architecture.md) | System architecture and design patterns |
| [Development Guide](development-guide.md) | Setup, testing, and contributing guidelines |

### Quick Links

- **Package on NuGet:** [nuget.org/packages/PackageScriptWriter.Cli](https://www.nuget.org/packages/PackageScriptWriter.Cli/)
- **Web Application:** [psw.codeshare.co.uk](https://psw.codeshare.co.uk)
- **Main Repository:** [github.com/prjseal/Package-Script-Writer](https://github.com/prjseal/Package-Script-Writer)
- **Umbraco Marketplace:** [marketplace.umbraco.com](https://marketplace.umbraco.com)

---

## 🤝 Contributing

We welcome contributions to the Package Script Writer CLI! Whether it's bug fixes, new features, documentation improvements, or testing, all contributions are appreciated.

### How to Contribute

1. **Check Existing Issues** - Browse [GitHub Issues](https://github.com/prjseal/Package-Script-Writer/issues) to see if your idea already exists
2. **Open a Discussion** - For new features, open an issue first to discuss the approach
3. **Fork the Repository** - Create your own fork to work in
4. **Create a Branch** - Use descriptive branch names (e.g., `feature/template-validation`)
5. **Make Your Changes** - Follow the existing code style and patterns
6. **Test Thoroughly** - Ensure your changes work as expected
7. **Submit a Pull Request** - Provide a clear description of your changes

### Development Setup

```bash
# Clone the repository
git clone https://github.com/prjseal/Package-Script-Writer.git
cd Package-Script-Writer

# Navigate to CLI project
cd src/PackageCliTool

# Run the CLI in development
dotnet run

# Run with arguments
dotnet run -- --help
dotnet run -- -p "uSync" -n TestProject

# Build and pack
dotnet pack -c Release

# Run tests (if available)
dotnet test
```

### Coding Guidelines

- **Follow C# conventions** - Use standard C# naming and formatting
- **Keep it modular** - Separate concerns into appropriate services and classes
- **Add comments** - Document complex logic and public APIs
- **Handle errors gracefully** - Provide user-friendly error messages
- **Test your changes** - Ensure nothing breaks and new features work correctly

### Areas Where We Need Help

- 🐛 **Bug Fixes** - Report or fix issues you encounter
- ✨ **New Features** - Add new capabilities to the CLI
- 📖 **Documentation** - Improve or expand documentation
- 🧪 **Testing** - Add unit tests and integration tests
- 🎨 **UI/UX** - Enhance the terminal interface
- 🌍 **Localization** - Add support for other languages

**[📖 Full Contributing Guide](development-guide.md)**

---

## 🐛 Reporting Issues

Found a bug or have a feature request? We want to hear from you!

### Before Reporting

1. **Check Existing Issues** - Search [GitHub Issues](https://github.com/prjseal/Package-Script-Writer/issues) to avoid duplicates
2. **Try Latest Version** - Update to the latest version: `dotnet tool update --global PackageScriptWriter.Cli`
3. **Enable Verbose Logging** - Run with `--verbose` flag to get detailed diagnostic output
4. **Check Log Files** - Review logs in `logs/psw-cli-{Date}.log`

### How to Report an Issue

Create a new issue on GitHub with the following information:

**For Bugs:**
- **Description** - Clear description of the problem
- **Steps to Reproduce** - Exact steps to reproduce the issue
- **Expected Behavior** - What should happen
- **Actual Behavior** - What actually happens
- **Environment** - OS, .NET version, CLI tool version
- **Logs** - Relevant log entries (use verbose mode)
- **Screenshots** - If UI-related, include screenshots

**For Feature Requests:**
- **Description** - Clear description of the feature
- **Use Case** - Why is this feature needed?
- **Proposed Solution** - How should it work?
- **Alternatives** - Any alternative approaches considered?

**For Security Issues:**
- **DO NOT** open a public issue for security vulnerabilities
- Report privately via GitHub Security Advisories
- See [Security Policy](cli/security.md#reporting-security-issues)

### Issue Templates

When creating an issue on GitHub, select the appropriate template:
- 🐛 **Bug Report** - For reporting bugs and errors
- ✨ **Feature Request** - For suggesting new features
- 📖 **Documentation** - For documentation improvements
- 🔒 **Security Issue** - For reporting security vulnerabilities (use private advisory)

**[Report an Issue on GitHub](https://github.com/prjseal/Package-Script-Writer/issues/new/choose)**

---

## 💬 Support & Community

### Getting Help

- **Documentation** - Check this documentation and the [README](../src/PackageCliTool/README.md)
- **GitHub Issues** - Search existing issues or create a new one
- **GitHub Discussions** - Ask questions and share ideas
- **Umbraco Community** - Connect with other Umbraco developers

### Troubleshooting

**Common Issues:**

**Tool not found after installation:**
```bash
# Ensure .NET tools path is in your PATH
# Linux/macOS: Add to ~/.bashrc or ~/.zshrc
export PATH="$PATH:$HOME/.dotnet/tools"

# Windows: Usually added automatically, or add manually:
# %USERPROFILE%\.dotnet\tools
```

**Package not found:**
- Verify package name is correct
- Check internet connectivity
- Try with `--verbose` flag for detailed API logs
- Ensure package exists on NuGet or Umbraco Marketplace

**API connection issues:**
- The API is only used for fetching package data
- Script generation is performed locally
- Check logs in `logs/` directory
- The tool automatically retries failed requests 3 times

**Enable verbose logging:**
```bash
# Option 1: Flag
psw --verbose

# Option 2: Environment variable
export PSW_VERBOSE=1  # Linux/macOS
set PSW_VERBOSE=1     # Windows CMD
$env:PSW_VERBOSE=1    # Windows PowerShell
```

**[📖 Full Troubleshooting Guide](../src/PackageCliTool/README.md#troubleshooting)**

### Community Resources

- **Umbraco Documentation:** [docs.umbraco.com](https://docs.umbraco.com)
- **Umbraco Community:** [community.umbraco.com](https://community.umbraco.com)
- **Umbraco Marketplace:** [marketplace.umbraco.com](https://marketplace.umbraco.com)

---

## 📝 License

This project is licensed under the **MIT License** - see the [LICENSE](../LICENSE) file for details.

### Third-Party Licenses

The CLI tool uses several open-source libraries:
- **Spectre.Console** - MIT License
- **Serilog** - Apache License 2.0
- **Polly** - BSD 3-Clause License
- **YamlDotNet** - MIT License

---

## 👨‍💻 Author

**Paul Seal**

- Website: [codeshare.co.uk](https://codeshare.co.uk)
- Twitter: [@codeshare](https://twitter.com/codeshare)
- GitHub: [@prjseal](https://github.com/prjseal)

---

## 🙏 Acknowledgments

- **Umbraco Community** - For the amazing CMS and package ecosystem
- **Microsoft** - For .NET and excellent tooling
- **Spectre.Console** - For the beautiful terminal UI library
- **Contributors** - Everyone who has contributed to this project
- **Beta Testers** - Thank you for testing and providing feedback!

---

## 📊 Project Stats

![GitHub stars](https://img.shields.io/github/stars/prjseal/Package-Script-Writer?style=social)
![GitHub forks](https://img.shields.io/github/forks/prjseal/Package-Script-Writer?style=social)
![NuGet Downloads](https://img.shields.io/nuget/dt/PackageScriptWriter.Cli.svg)

---

<div align="center">

**⭐ If this tool helps you, consider giving it a star! ⭐**

Made with ❤️ for the Umbraco Community

[Back to Top ↑](#package-script-writer-cli-documentation)

</div>
