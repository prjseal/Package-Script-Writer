# Changelog

All notable changes to Package Script Writer CLI will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- **Non-interactive Mode (`--save-only`)** - Save the generated script to a file (via `--output-file <file>`) and exit immediately without showing interactive prompts. Enables programmatic use from other tools (e.g., MCP servers, CI/CD pipelines) where stdin is not a TTY
- **Build-only Mode (`--no-build`)** - Skip the `dotnet run` command from the generated script. When combined with `--auto-run`, this allows installing and building an Umbraco project without starting the web server. Previously named `--no-run`
- **Script Output File (`--output-file <file>`)** - Specify an output file path for the generated script, used with `--save-only`
- **Machine-Readable Output (`--output json|plain`)** - New output format flag for AI agents and automation. JSON mode returns structured responses with script, configuration, and metadata. Plain mode outputs raw text with no ANSI codes or decoration
- **Script-Only Output (`--script-only`)** - Outputs only the raw generated script with no panels, spinners, or status messages. Ideal for piping to files or other tools
- **Non-Interactive Mode (`--no-interaction`)** - Suppresses all interactive prompts. In CLI mode, the tool will output the script and exit without asking "What would you like to do?"
- **Dry Run (`--dry-run`)** - Validates all inputs and displays the resolved configuration without generating a script or calling any APIs
- **Structured Help (`--help-json`)** - Outputs all commands, options, types, defaults, and valid values as structured JSON. Enables AI agents to programmatically discover CLI capabilities
- **List Options Command (`psw list-options`)** - New subcommand to list valid values for CLI options. Supports categories: `database-types`, `starter-kits`, `defaults`. Works with `--output json` for machine-readable output
- **Template Version Flag (`--template-version`)** - Explicit flag for setting template version as an alternative to the pipe syntax in `--template-package`
- **Distinct Exit Codes** - Different exit codes for different error categories: 0 (success), 1 (general), 2 (validation), 3 (network), 4 (script execution), 5 (file system)
- **Structured Version Output** - `psw --version --output json` returns version, runtime, and platform as JSON. `--output plain` returns just the version number
- **JSON Error Responses** - When using `--output json`, errors are returned as structured JSON with error message, error code, and suggestion fields

### Changed
- **Unified `-t` and `--template-package` Flags** - Both `-t` and `--template-package` now behave identically: bare values are treated as package names, and `Name|Version` pipe syntax sets both. Previously `-t` treated bare values as version only
- **Improved Help Text** - All options now show their type (string, enum, flag), default values, and dependency notes directly in help output. New "AI Agent / Automation" examples section added
- **Stderr/Stdout Separation** - In `--output plain` and `--script-only` modes, status messages go to stderr while the script goes to stdout, enabling clean piping

## [1.1.2] - 2026-01-16

### Fixed
- **Process Cleanup on Exit** - Fixed issue where `dotnet run` processes spawned by scripts would continue running in the background after the CLI tool exited
  - Added automatic tracking and cleanup of all spawned processes
  - Implemented graceful shutdown handlers for Ctrl+C and application exit
  - Process trees are now properly terminated on both Windows and Unix systems

## [1.1.1] - 2026-01-08

### Added
- **Exit Option in Interactive Mode** - Added "Exit" option to the main menu for graceful application exit

## [1.1.0] - 2026-01-08

### Added
- **Enhanced Package Selection System** - Completely redesigned package selection workflow with multiple discovery methods
  - New main menu with 5 options for adding packages
  - "Select from popular Umbraco packages" - Browse all marketplace packages with multi-select
  - "Search for package on Umbraco Marketplace" - Search by keyword in marketplace
  - "Search for package on nuget.org" - Full NuGet.org search integration via API
  - "Modify selected packages" - Review and remove previously selected packages
  - "Done - finish package selection" - Exit package selection loop
- **Cancel Options** - Added "Cancel" option at the top of all package selection lists to return to main menu
- **Immediate Version Selection** - Version selection now happens immediately after selecting each package (instead of batched after all selections)
- **Modify Selected Packages Feature** - New screen to review and remove selected packages
  - All packages displayed pre-selected with two-line format (Package ID on first line, version on second line with dash prefix)
  - Can uncheck packages to remove them
  - Supports removing all packages (multi-select with `Required(false)`)
- **NuGet.org Search Integration** - Direct search of NuGet.org repository via API
  - Searches packages beyond Umbraco Marketplace
  - Returns up to 20 results
  - Displays package ID, truncated description (100 chars), and authors
  - Excludes pre-release packages by default

### Changed
- **Package Selection Menu Structure** - Reorganized from 3 options to 5-option main loop
  - Clearer menu option names: "Select from popular Umbraco packages" and "Search for package on Umbraco Marketplace"
  - All cancel operations now return to the main package selection menu instead of aborting
  - Package selection is now a continuous loop until user selects "Done"
- **Version Display in Modify Screen** - Changed from single-line format `PackageId (Version)` to two-line format with dash prefix
  ```
  PackageId
    - Version
  ```

### Fixed
- Fixed compilation error in `ModifySelectedPackagesAsync` where `.Select()` was incorrectly called with a collection instead of individual items
- Fixed issue where users could not press Enter if all packages were unchecked in the modify screen

## [1.0.1] - 2026-01-05

### Changed
- **Community Templates API** - Migrated community templates data source from GitHub API to Package Script Writer API for improved performance and reliability

## [1.0.0] - 2025-12-18

### Added
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

[Unreleased]: https://github.com/prjseal/Package-Script-Writer/compare/v1.1.1...HEAD
[1.1.1]: https://github.com/prjseal/Package-Script-Writer/compare/v1.1.0...v1.1.1
[1.1.0]: https://github.com/prjseal/Package-Script-Writer/compare/v1.0.1...v1.1.0
[1.0.1]: https://github.com/prjseal/Package-Script-Writer/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/prjseal/Package-Script-Writer/releases/tag/v1.0.0
