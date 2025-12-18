# Package Script Writer CLI

[![NuGet](https://img.shields.io/nuget/v/PackageScriptWriter.Cli.svg)](https://www.nuget.org/packages/PackageScriptWriter.Cli/)
[![Downloads](https://img.shields.io/nuget/dt/PackageScriptWriter.Cli.svg)](https://www.nuget.org/packages/PackageScriptWriter.Cli/)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)

An interactive command-line tool for generating Umbraco CMS installation scripts. Features a beautiful terminal UI built with Spectre.Console, supports 500+ Marketplace packages, and offers both interactive and automation modes.

## Quick Start

```bash
# Install globally from NuGet
dotnet tool install --global PackageScriptWriter.Cli

# Launch interactive mode
psw

# Or use CLI mode for automation
psw --default
psw -p "uSync,Diplo.GodMode" -n MyProject
```

## Key Features

- 🎨 **Beautiful Terminal UI** - Built with Spectre.Console
- 🚀 **Dual Mode** - Interactive prompts OR command-line automation
- 📦 **500+ Packages** - Browse Umbraco Marketplace packages
- 💾 **Templates** - Save and reuse configurations
- 📊 **History** - Track all generated scripts
- 🔒 **Secure** - Command validation and password protection
- 🐳 **Docker Ready** - Optional Dockerfile generation

## Installation

### From NuGet (Recommended)

```bash
dotnet tool install --global PackageScriptWriter.Cli
```

### From Source

```bash
git clone https://github.com/prjseal/Package-Script-Writer.git
cd Package-Script-Writer/src
dotnet pack PackageCliTool -c Release
dotnet tool install --global --add-source ./PackageCliTool/bin/Release PackageScriptWriter.Cli
```

## Usage

### Interactive Mode

```bash
psw
```

Navigate through the menu-driven interface:
1. Use default script
2. Use local template
3. Use community template
4. Load script from history
5. Create new script
6. Load Umbraco versions table
7. Help

### CLI Mode Examples

```bash
# Default script
psw --default

# Custom packages with specific versions
psw -p "uSync|17.0.0,Diplo.GodMode" -n MyBlog

# Full automation with unattended install
psw -p "uSync" -n MyProject -s MySolution \
    -u --database-type SQLite \
    --admin-email admin@example.com \
    --admin-password "SecurePass123!" \
    --auto-run
```

### Template Commands

```bash
psw template save <name>        # Save current config as template
psw template load <name>        # Load saved template
psw template list               # List all templates
psw template delete <name>      # Delete template
```

### History Commands

```bash
psw history list                # View recent scripts
psw history show <#>            # Show script details
psw history rerun <#>           # Re-run previous script
```

## Requirements

- .NET 10.0 SDK or later
- Internet connection (for package information)

## Documentation

📚 **Complete Documentation**: [CLI Tool Documentation](../../.github/cli-tool-documentation.md)

### Quick Links

- **[Interactive Mode Guide](../../.github/cli/interactive-mode.md)** - Complete walkthrough
- **[Templates Guide](../../.github/cli/templates.md)** - Template system
- **[History Guide](../../.github/cli/history.md)** - History feature
- **[Security Guide](../../.github/cli/security.md)** - Security features
- **[CLI Reference](../../.github/cli-documentation.md)** - Full CLI documentation

## Support

- **Issues**: [GitHub Issues](https://github.com/prjseal/Package-Script-Writer/issues)
- **Discussions**: [GitHub Discussions](https://github.com/prjseal/Package-Script-Writer/discussions)
- **Website**: [psw.codeshare.co.uk](https://psw.codeshare.co.uk)

## Version

**Current Version**: 1.0.0 (Stable)

**Release Notes**: [Release History](../../.github/cli/release-notes.md)

## License

MIT License - see [LICENSE](../../LICENSE) file for details.

## Author

**Paul Seal**
- Website: [codeshare.co.uk](https://codeshare.co.uk)
- GitHub: [@prjseal](https://github.com/prjseal)

---

<div align="center">

**⭐ If this tool helps you, consider giving it a star! ⭐**

[Documentation](../../.github/cli-tool-documentation.md) · [Issues](https://github.com/prjseal/Package-Script-Writer/issues) · [Discussions](https://github.com/prjseal/Package-Script-Writer/discussions)

</div>
