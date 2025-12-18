# Custom Script Configuration Guide

## Overview

The custom script option gives you complete control over every aspect of your Umbraco installation script. Build exactly what you need with full customization of packages, database, Docker, and more.

## Accessing Custom Script Builder

### Interactive Mode
```bash
psw
# Select "Create new script" from the main menu
```

## Configuration Editor

When you select "Create new script", you'll see the configuration editor with all available options:

```
Configuration Editor

Select configuration options to edit (use Space to select, Enter to confirm):
> [X] Template & Project Settings
  [X] Packages
  [ ] Starter Kit
  [ ] Docker Options
  [ ] Unattended Install
  [ ] Output Format

  Generate Script
  Cancel
```

Select the options you want to configure using **Space**, then press **Enter**.

## Configuration Options

### 1. Template & Project Settings

Configure the Umbraco template and project structure:

**Template Package**:
- Umbraco.Templates (official)
- Umbraco.Community.Templates.Clean
- Umbraco.Community.Templates.UmBootstrap
- Custom template package

**Template Version**:
- Latest Stable
- Latest LTS (Long Term Support)
- Specific version number

**Project Settings**:
- Project name (alphanumeric, no spaces)
- Create solution file (yes/no)
- Solution name (if creating solution)

**Example:**
```
Select Umbraco template package:
> Umbraco.Templates

Select template version:
> Latest LTS

Enter project name: MyCustomProject
Create a solution file? y
Enter solution name: MyCustomSolution
```

### 2. Packages

Browse and select from 500+ Umbraco Marketplace packages:

**Selection**:
- Use **Space** to select/deselect packages
- Use **↑/↓** to navigate
- Type to search/filter packages
- Select **"Add custom package..."** to enter a package name manually

**Version Selection**:
- For each selected package, choose a version
- Options: Latest, Pre-release, or specific version

**Example:**
```
Select packages:
  [X] uSync
  [X] Diplo.GodMode
  [X] Umbraco.Forms
  [ ] Umbraco.Community.BlockPreview

Select version for uSync:
> 17.0.0 (latest stable)
  17.0.0-beta1
  16.0.0
```

### 3. Starter Kit

Add a starter kit for initial content and structure:

**Available Starter Kits**:
- **clean** - Minimal starter with basic content types
- **Articulate** - Blog platform
- **Portfolio** - Portfolio starter template
- **LittleNorth.Igloo** - Igloo starter kit
- **Umbraco.BlockGrid.Example.Website** - Block Grid examples
- **Umbraco.TheStarterKit** - Complete example site
- **uSkinnedSiteBuilder** - uSkinned site builder

**Example:**
```
Include a starter kit? y

Select starter kit:
> clean

Select version:
> Latest stable
```

### 4. Docker Options

Configure Docker support for containerization:

**Options**:
- **Dockerfile** - Creates a Dockerfile for building container images
- **Docker Compose** - Creates docker-compose.yml for multi-container setup

**Example:**
```
Include Dockerfile? y
Include Docker Compose file? y
```

Generates both files configured for Umbraco with:
- .NET 10.0 runtime
- SQL Server connection support
- Volume mounts for data persistence
- Environment variable configuration

### 5. Unattended Install

Configure automatic installation without manual setup wizard:

**Database Configuration**:
- **SQLite** - File-based, good for development
- **LocalDb** - SQL Server LocalDB
- **SQLServer** - Full SQL Server (requires connection string)
- **SQLAzure** - Azure SQL Database (requires connection string)
- **SQLCE** - SQL Server Compact Edition

**Admin User**:
- Friendly name (display name)
- Email address
- Password (hidden input, minimum 10 characters)

**Example:**
```
Use unattended install? y

Select database type:
> SQLite

Enter admin user friendly name: Site Administrator
Enter admin email: admin@mysite.com
Enter admin password: ********** (min 10 chars)
```

For SQL Server/Azure:
```
Select database type:
> SQL Server

Enter connection string:
Server=localhost;Database=MyUmbraco;User Id=sa;Password=...

Enter admin user friendly name: Site Administrator
...
```

### 6. Output Format

Control how the generated script is formatted:

**One-liner Output**:
- **No** (default) - Multi-line, readable format
- **Yes** - Single line, good for copy-paste

**Remove Comments**:
- **No** (default) - Includes explanatory comments
- **Yes** - Pure commands only

**Include Prerelease**:
- **No** (default) - Stable packages only
- **Yes** - Include pre-release package versions

**Example:**
```
Output as one-liner? n
Remove comments from script? n
Include prerelease packages? n
```

## Workflow

### Step-by-Step Process

1. **Select Configuration Areas**
   - Choose which settings to configure
   - Unselected areas use defaults

2. **Configure Selected Areas**
   - Work through each selected area
   - Answer prompts for each setting

3. **Review Configuration Summary**
   - See all settings in a table
   - Verify everything is correct

4. **Generate Script**
   - Confirm generation
   - Script is created instantly

5. **Choose Action**
   - Display, save, run, edit, or save as template

### Configuration Summary

Before generation, you'll see a complete summary:

```
Configuration Summary

┌─────────────────────┬──────────────────────────┐
│ Setting             │ Value                    │
├─────────────────────┼──────────────────────────┤
│ Template            │ Umbraco.Templates @ LTS  │
│ Project Name        │ MyCustomProject          │
│ Solution Name       │ MyCustomSolution         │
│ Packages            │ 3 package(s) selected    │
│ Starter Kit         │ clean                    │
│ Docker              │ Enabled (both files)     │
│ Unattended Install  │ Enabled                  │
│ Database Type       │ SQLite                   │
│ Admin User          │ Site Administrator       │
│ Admin Email         │ admin@mysite.com         │
│ Output Format       │ Multi-line with comments │
└─────────────────────┴──────────────────────────┘

Generate script with these settings? (y/n):
```

## Advanced Features

### Editing Configuration

After generating a script, you can edit and regenerate:

1. Select **"Edit configuration"** from script actions menu
2. Multi-select which fields to modify
3. Make changes
4. Review updated summary
5. Generate new script

### Incremental Configuration

You don't have to configure everything at once:

1. Configure basic settings (template & project)
2. Generate script
3. Select "Edit configuration"
4. Add more settings (packages, Docker, etc.)
5. Regenerate

### Saving as Template

Save successful configurations for reuse:

```
Select "Save as template" from script actions

Enter template name: my-production-setup
Enter description: Production-ready setup with all tools
Enter tags: production,complete,umbraco14

✓ Template saved to ~/.psw/templates/my-production-setup.yaml
```

Now load anytime with:
```bash
psw template load my-production-setup
```

## Examples

### Example 1: Blog with Forms

```bash
psw  # Start interactive mode

# Select "Create new script"

# Select configuration options:
# [X] Template & Project Settings
# [X] Packages
# [X] Starter Kit
# [ ] Docker Options
# [X] Unattended Install
# [ ] Output Format

# Template: Umbraco.Templates (Latest Stable)
# Project: "MyBlog"
# Solution: "MyBlog"

# Packages: uSync, Umbraco.Forms, Diplo.GodMode

# Starter Kit: clean (latest)

# Unattended:
#   Database: SQLite
#   Admin: "Blog Admin" <admin@myblog.com>
#   Password: **********

# Generate → Save → Run
```

### Example 2: E-Commerce Setup

```bash
psw

# Select "Create new script"

# Configure:
# - Template: Umbraco.Templates @ LTS
# - Project: "MyStore" with solution
# - Packages: uSync, Umbraco.Commerce, Umbraco.Forms
# - Starter Kit: None
# - Docker: Both (Dockerfile + Compose)
# - Unattended: SQL Server with connection string
# - Output: Standard format

# Save as template: "ecommerce-standard"
```

### Example 3: Minimal Development Setup

```bash
psw

# Select "Create new script"

# Configure only:
# - Template & Project Settings
#   - Template: Umbraco.Templates (Latest Stable)
#   - Project: "DevTest"
#   - No solution

# Leave everything else as defaults
# Generate → Run immediately
```

## Best Practices

### 1. Start Simple
Begin with just template and project settings. Add complexity as needed.

### 2. Use Starter Kits for Learning
Include a starter kit if you're new to Umbraco. The "clean" starter kit provides good examples.

### 3. Save Successful Configs
When you create a configuration that works well, save it as a template.

### 4. Use SQLite for Development
For local development, SQLite is easiest. Use SQL Server for production.

### 5. Enable Docker for Deployment
If deploying to containers, include Docker configuration from the start.

### 6. Keep Comments in Scripts
Unless generating for automation, keep comments enabled for clarity.

## Troubleshooting

### Configuration Too Complex
**Problem**: Too many options, feeling overwhelmed

**Solution**:
- Start with default script
- Use "Edit configuration" to add one thing at a time
- Or use a community template as a starting point

### Package Not Found
**Problem**: Selected package doesn't exist

**Solution**:
- Check spelling
- Try searching on nuget.org
- Package may have been renamed or deprecated

### Invalid Project Name
**Problem**: Project name rejected

**Solution**:
- Use alphanumeric characters only
- No spaces (use PascalCase or hyphens)
- Can't start with a number

### Database Connection Failed
**Problem**: Unattended install fails to connect to database

**Solution**:
- Verify connection string is correct
- Ensure database server is running
- Check firewall settings
- Test connection separately

## Related Documentation

- **[Default Script](default-script.md)** - Quick default setup
- **[Templates Guide](templates.md)** - Save and reuse configurations  
- **[Interactive Mode](interactive-mode.md)** - Complete interactive mode guide
- **[CLI Documentation](../cli-documentation.md)** - Full CLI reference

---

<div align="center">

**🏠 [Back to Interactive Mode](interactive-mode.md)** | **📖 [CLI Documentation](../cli-documentation.md)**

</div>
