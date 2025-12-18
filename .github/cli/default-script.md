# Default Script Guide

## Overview

The default script option provides the fastest way to generate an Umbraco installation script with sensible, production-ready defaults. Perfect for quick setups and getting started.

## Accessing Default Script

### Interactive Mode
```bash
psw
# Select "Use default script" from the main menu
```

### CLI Mode
```bash
psw --default
# or
psw -d
```

## What's Included

The default script generates:

### Template
- **Package**: Umbraco.Templates
- **Version**: Latest stable release
- **Type**: Official Umbraco template

### Project
- **Name**: MyProject
- **Solution**: No solution file created
- **Structure**: Single project

### Starter Kit
- **Package**: clean
- **Version**: Latest stable
- **Purpose**: Provides basic content types and structure

### Database
- **Type**: None (manual configuration during first run)
- **Unattended Install**: Disabled
- **Setup**: Interactive setup wizard on first launch

### Packages
- **Additional packages**: None
- **Rationale**: Keep it minimal, add what you need later

### Output Format
- **Style**: Multi-line with comments
- **Comments**: Included (explains each step)
- **Format**: Readable bash/PowerShell script

## Generated Script Example

```bash
# Install Umbraco templates
dotnet new install Umbraco.Templates --force

# Create new Umbraco project
dotnet new umbraco --force -n "MyProject"

# Add clean starter kit
dotnet add "MyProject" package clean

# Run the project
dotnet run --project "MyProject"
```

## When to Use Default Script

✅ **Use default script when:**
- You want to get started quickly
- You're new to Umbraco
- You're testing or learning
- You want minimal configuration
- You'll customize the project later

❌ **Don't use default script when:**
- You need specific packages installed
- You want unattended/automated setup
- You need Docker configuration
- You have specific database requirements
- You're deploying to production

## Customizing Default Script

While the default script uses fixed settings, you can customize it after generation:

### Option 1: Edit Configuration
After generation, select **"Edit configuration"** from the script actions menu to modify settings and regenerate.

### Option 2: Override with CLI Flags
Use the default script as a base but override specific settings:

```bash
# Default script with custom project name
psw --default -n MyBlog

# Default script with additional packages
psw --default -p "uSync,Diplo.GodMode"

# Default script with solution
psw --default -n MyProject -s MySolution
```

### Option 3: Save as Template
After editing the default configuration, save it as a template for reuse:

1. Generate default script
2. Select "Edit configuration"
3. Make your changes
4. Generate the script
5. Select "Save as template"
6. Name it (e.g., "my-default")

Now you can quickly load your customized default anytime.

## Script Actions

After generating the default script, you can:

### Display in Terminal
View the full script with syntax highlighting in your terminal.

### Save to File
```
Enter file name [install-script.sh]: quick-start.sh
✓ Script saved to quick-start.sh
```

Save to your preferred filename for later use.

### Run Immediately
Execute the script right away in a specified directory:

```
Enter directory to run script in [current directory]: ./my-project
⠋ Running script...
✓ Script completed successfully!
```

### Edit Configuration
Modify the configuration and regenerate:
- Add packages
- Change project name
- Enable unattended install
- Add Docker support
- Modify starter kit

### Save as Template
Save your configuration for reuse:

```
Enter template name: my-quick-start
Enter description: My customized default setup
✓ Template saved
```

## Comparison with Other Options

| Feature | Default Script | Custom Script | Template | Community Template |
|---------|---------------|---------------|----------|-------------------|
| **Speed** | ⚡⚡⚡ Instant | 🐢 Slow (manual) | ⚡⚡ Fast | ⚡⚡ Fast |
| **Customization** | ❌ Minimal | ✅ Full control | ✅ Pre-configured | ⚡ Limited |
| **Packages** | Clean kit only | Any packages | Pre-selected | Pre-selected |
| **Best For** | Quick starts | Complex setups | Repeated setups | Common scenarios |

## Examples

### Example 1: Quick Test Setup
```bash
# Generate and run immediately
psw --default

# Select "Run immediately"
# Enter directory: ./test-site
# Done! Umbraco running locally
```

### Example 2: Save for Later
```bash
# Generate default script
psw --default

# Select "Save to file"
# Enter filename: umbraco-base.sh
# Share with team or save for later
```

### Example 3: Build on Default
```bash
# Start with default
psw --default

# Select "Edit configuration"
# Add packages: uSync, Diplo.GodMode
# Enable unattended install
# Generate new script
# Select "Save as template"
# Name: "my-enhanced-default"
```

## Troubleshooting

### Clean Package Not Found
```
⚠ Package 'clean' not found
```

**Solution**: The clean starter kit may have been updated. Check the latest version:
```bash
dotnet search clean --take 5
```

### Template Already Installed
```
⚠ Template already installed
```

**Solution**: This is normal. The `--force` flag ensures it's updated to the latest version.

### Project Already Exists
```
✗ Error: Directory 'MyProject' already exists
```

**Solution**:
- Use a different project name: `psw --default -n MyProject2`
- Delete or rename the existing directory
- Run in a different location

## Related Documentation

- **[Custom Script Guide](custom-script.md)** - Build fully customized scripts
- **[Templates Guide](templates.md)** - Save and reuse configurations
- **[Interactive Mode](interactive-mode.md)** - Full interactive mode guide
- **[CLI Documentation](../cli-documentation.md)** - Complete CLI reference

---

<div align="center">

**🏠 [Back to Interactive Mode](interactive-mode.md)** | **📖 [CLI Documentation](../cli-documentation.md)**

</div>
