# Package Script Writer CLI - Release Notes

## Version 1.0.0 (2025-12-18)

**🎉 Initial Stable Release**

This is the first stable release of the Package Script Writer CLI tool!

### ✨ Features

- **Interactive Mode** - Beautiful terminal UI with Spectre.Console
  - Browse 500+ Umbraco Marketplace packages
  - Multi-select package selection with search
  - Version selection for each package
  - Template selection with version control
  - Complete configuration workflow

- **CLI Mode** - Full automation support with command-line flags
  - All configuration options available as flags
  - Perfect for CI/CD pipelines and scripts
  - One-liner script generation
  - Auto-run capabilities

- **Script Generation** - Generate complete Umbraco installation scripts
  - Umbraco template installation
  - Project and solution creation
  - Package installation with version pinning
  - Starter kit support (9 different kits)
  - Docker integration (Dockerfile & Docker Compose)
  - Unattended install configuration
  - Database setup (SQLite, LocalDb, SQL Server, SQL Azure)
  - Admin user credentials

- **Template System** - Save and reuse configurations
  - Save templates from any configuration
  - Load templates with overrides
  - Export/import for team sharing
  - YAML-based configuration files

- **History Tracking** - Automatic tracking of all generated scripts
  - View recent history
  - Re-run previous scripts
  - Track execution status
  - Audit trail

- **Resilience & Logging**
  - Automatic retry logic with exponential backoff (Polly)
  - Comprehensive logging with Serilog
  - Verbose mode for debugging
  - File and console logging

- **Security**
  - Command allowlist validation
  - Secure password input
  - No script storage (regenerated from config)

### 📦 Package Information

- **Package ID**: PackageScriptWriter.Cli
- **Command**: `psw`
- **Target Framework**: .NET 10.0
- **NuGet**: [nuget.org/packages/PackageScriptWriter.Cli](https://www.nuget.org/packages/PackageScriptWriter.Cli/)

### 📝 Documentation

Complete documentation available:
- [CLI Documentation Hub](../cli-documentation.md) - Main documentation
- [README.md](../../src/PackageCliTool/README.md) - Technical reference
- [Templates Guide](templates.md) - Template system guide
- [History System](history.md) - History feature documentation
- [Security Guide](security.md) - Security features

### 🙏 Acknowledgments

Thank you to everyone who tested the beta versions and provided feedback!

---

## Future Releases

Future version history will be documented here as new releases are published.
