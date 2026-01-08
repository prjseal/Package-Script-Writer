# Interactive Mode Guide

## Overview

Interactive mode is the default mode when you run `psw` without any command-line flags. It provides a beautiful, menu-driven interface with step-by-step prompts to guide you through generating Umbraco installation scripts.

## Starting Interactive Mode

Simply type `psw` at the command line:

```bash
psw
```

## Welcome Screen

When you start interactive mode, you'll see the welcome banner:

```
 ____  ______        __  ____ _     ___
|  _ \/ ___\ \      / / / ___| |   |_ _|
| |_) \___ \\ \ /\ / / | |   | |    | |
|  __/ ___) |\ V  V /  | |___| |___ | |
|_|   |____/  \_/\_/    \____|_____|___|

Package Script Writer - Interactive CLI
By Paul Seal
Press Ctrl+C at any time to start over
```

The CLI automatically checks for package updates and populates the marketplace cache in the background.

## Main Menu

After the welcome screen, you'll see the main menu with these options:

```
What would you like to do?
> Use default script
  Use local template
  Use community template
  Load script from history
  Create new script
  Load Umbraco versions table
  Help
```

Use the **arrow keys** (↑/↓) to navigate and **Enter** to select an option.

---

## Menu Options

### 1. Use Default Script

**Description**: Generates a script with sensible defaults - perfect for quick setup.

**What you see:**
```
Create Script with Default Configuration

Using default configuration (latest stable Umbraco with clean starter kit)

⠋ Generating default installation script...

╔═══════════════════════════════════════════╗
║ Generated Default Installation Script    ║
╠═══════════════════════════════════════════╣
║                                           ║
║ # Install Umbraco templates               ║
║ dotnet new install Umbraco.Templates...   ║
║ dotnet new umbraco --force...             ║
║ dotnet add package clean...               ║
║                                           ║
╚═══════════════════════════════════════════╝

What would you like to do with this script?
(Press Ctrl+C at any time to start over)
> Run
  Edit
  Copy
  Save
  Start over
```

**Default configuration includes:**
- Latest stable Umbraco Templates
- Project name: "MyProject"
- Clean starter kit package
- No unattended install (requires manual setup)
- SQLite database (if unattended)
- Standard multi-line format

**📖 Learn more**: [Default Script Guide](default-script.md)

---

### 2. Use Local Template

**Description**: Load a saved template configuration from your local template library.

**What you see:**
```
Load template

Select a template:
> my-blog-template - My blog setup
  company-standard - Company standard configuration
  client-premium - Premium client setup
  e-commerce-setup - E-commerce configuration
```

After selecting a template:
```
⠋ Loading template my-blog-template...

✓ Template loaded: my-blog-template

⠋ Generating installation script...

╔═══════════════════════════════════════════╗
║ Generated Installation Script            ║
╠═══════════════════════════════════════════╣
║                                           ║
║ # Install Umbraco templates               ║
║ dotnet new install Umbraco.Templates...   ║
║ dotnet new umbraco --force...             ║
║ dotnet add package uSync|17.0.0...        ║
║ dotnet add package Umbraco.Forms...       ║
║                                           ║
╚═══════════════════════════════════════════╝

What would you like to do with this script?
(Press Ctrl+C at any time to start over)
> Run
  Edit
  Copy
  Save
  Start over
```

**Features:**
- Quick access to your saved configurations
- Automatically generates script from template
- Edit and regenerate from action menu if needed
- Save new templates from any configuration

**📖 Learn more**: [Templates Guide](templates.md)

---

### 3. Use Community Template

**Description**: Browse and use pre-made templates shared by the Umbraco community.

**What you see:**
```
Community Templates
Choose from templates shared by the community

⠋ Fetching community templates...

Select a community template:
> Umbraco 14 Blog (blog, umbraco14)
  Complete blog setup with uSync and Umbraco Forms

  Umbraco 14 E-Commerce Starter (e-commerce, umbraco14)
  E-commerce setup with payment integration

  Umbraco 13 LTS Standard (lts, umbraco13)
  Standard Umbraco 13 LTS configuration
```

After selecting a template:
```
⠋ Loading template 'Umbraco 14 Blog'...

✓ Loaded: Umbraco 14 Blog
  Author: Community Contributors
  Complete blog setup with uSync and Umbraco Forms

⠋ Generating installation script...

╔═══════════════════════════════════════════════════╗
║ Generated Script from 'Umbraco 14 Blog'          ║
╠═══════════════════════════════════════════════════╣
║                                                   ║
║ # Install Umbraco templates                       ║
║ dotnet new install Umbraco.Templates...           ║
║ dotnet new umbraco --force...                     ║
║ dotnet add package uSync|17.0.0...                ║
║ dotnet add package Umbraco.Forms|14.2.0...        ║
║                                                   ║
╚═══════════════════════════════════════════════════╝

What would you like to do with this script?
(Press Ctrl+C at any time to start over)
> Run
  Edit
  Copy
  Save
  Start over
```

**Features:**
- Community-maintained templates hosted on GitHub
- Pre-configured for common scenarios
- Automatically generates script from template
- Edit and regenerate from action menu if needed

**📖 Learn more**: [Community Templates Guide](community-templates.md)

---

### 4. Load Script from History

**Description**: View and re-run scripts you've generated before.

**What you see:**
```
Load script from history

Select a history entry:
> 2025-12-18 14:30 - MyBlog
  2025-12-18 10:15 - ClientSite
  2025-12-17 16:45 - TestProject
  2025-12-17 09:00 - DevSetup
```

After selecting an entry:
```
✓ History entry loaded: MyBlog

⠋ Generating installation script...

╔═══════════════════════════════════════════╗
║ Generated Installation Script            ║
╠═══════════════════════════════════════════╣
║                                           ║
║ # Install Umbraco templates               ║
║ dotnet new install Umbraco.Templates...   ║
║ dotnet new umbraco --force...             ║
║ dotnet add package uSync|17.0.0...        ║
║ dotnet add package Diplo.GodMode...       ║
║                                           ║
╚═══════════════════════════════════════════╝

What would you like to do with this script?
(Press Ctrl+C at any time to start over)
> Run
  Edit
  Copy
  Save
  Start over
```

**Features:**
- Automatically tracks all generated scripts
- Shows timestamp and project name
- Regenerates scripts with same configuration
- Edit and regenerate from action menu if needed

**📖 Learn more**: [History System Guide](history.md)

---

### 5. Create New Script

**Description**: Build a custom script from scratch with full control over all options.

**What you see:**
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

#### If you select "Template & Project Settings":

```
Template & Project Settings

Select Umbraco template package:
> Umbraco.Templates (official)
  Umbraco.Community.Templates.Clean
  Umbraco.Community.Templates.UmBootstrap
  Custom template package...

Select template version:
> Latest Stable
  Latest LTS (14.x)
  Specific version...

Enter project name [MyProject]: MyCustomProject

Create a solution file? (y/n): y

Enter solution name [MyCustomProject]: MyCustomSolution
```

#### If you select "Packages":

The package selection system offers multiple ways to find and add packages, with version selection happening immediately after each package is selected.

```
Step 3: Select Packages and Versions

How would you like to add packages?
> Select from popular Umbraco packages
  Search for package on Umbraco Marketplace
  Search for package on nuget.org
  Modify selected packages
  Done - finish package selection
```

**Option 1: Select from popular Umbraco packages**

Browse all packages from the Umbraco Marketplace with a cancel option at the top:

```
Select packages (use Space to select, Enter to confirm):
> Cancel - don't add any of these packages
  [ ] Umbraco.Community.BlockPreview
  [ ] Diplo.GodMode
  [ ] uSync
  [ ] Umbraco.Community.Contentment
  [ ] Umbraco.Forms
  [ ] Artikulate
  [ ] Our.Umbraco.GMaps
  ... (500+ packages available)
  (Page 1 of 50)
```

**Option 2: Search for package on Umbraco Marketplace**

Search for specific packages by name with a cancel option at the top:

```
Enter search term: god mode

Search results:
> Cancel - don't add any of these packages
  Diplo.GodMode - God Mode for Umbraco
  (1 result found)
```

**Option 3: Search for package on nuget.org**

Search the full NuGet.org repository for packages not in the Umbraco Marketplace:

```
Enter search term: newtonsoft

Search results (from nuget.org):
> Cancel - don't add any of these packages
  Newtonsoft.Json - Popular JSON framework for .NET (by James Newton-King)
  Newtonsoft.Json.Schema - JSON Schema reader, generator, and validator (by James Newton-King)
  ... (showing up to 20 results)
```

**Immediate version selection after each package:**

After selecting any package, you immediately choose its version:

```
Select version for Diplo.GodMode:
> Latest Stable
  Pre-release
  3.0.3
  3.0.2
  3.0.1
  3.0.0
```

**Option 4: Modify selected packages**

Review and remove previously selected packages. All packages are pre-selected:

```
Select packages to keep (uncheck to remove):
  [X] Diplo.GodMode
      - 3.0.3
  [X] uSync
      - Latest Stable
  [X] Newtonsoft.Json
      - 13.0.3

(Use Space to toggle, Enter to confirm)
```

You can uncheck any package to remove it, or uncheck all to clear your selections.

**Option 5: Done - finish package selection**

Returns to the configuration menu with all selected packages and their versions saved.

#### If you select "Starter Kit":

```
Starter Kit Options

Include a starter kit? (y/n): y

Select starter kit:
> clean
  Articulate
  Portfolio
  LittleNorth.Igloo
  Umbraco.BlockGrid.Example.Website
  Umbraco.TheStarterKit
  uSkinnedSiteBuilder

Select starter kit version:
> Latest stable
  Specific version...
```

#### If you select "Docker Options":

```
Docker Options

Include Dockerfile? (y/n): y

Include Docker Compose file? (y/n): y
```

#### If you select "Unattended Install":

```
Unattended Install Options

Use unattended install? (y/n): y

Select database type:
> SQLite
  LocalDb
  SQLServer
  SQLAzure
  SQLCE

[If SQLServer/SQLAzure selected]
Enter connection string:

Enter admin user friendly name [Administrator]: Site Admin

Enter admin email [admin@example.com]: admin@mysite.com

Enter admin password (min 10 characters): **********
```

#### If you select "Output Format":

```
Output Format Options

Output as one-liner? (y/n): n

Remove comments from script? (y/n): n

Include prerelease packages? (y/n): n
```

#### Configuration Summary:

After selecting your options:

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
│ Admin User          │ Site Admin               │
│ Admin Email         │ admin@mysite.com         │
│ Output Format       │ Multi-line with comments │
└─────────────────────┴──────────────────────────┘

Generate script with these settings? (y/n): y

⭐ Generating installation script...

[Script is displayed in a panel]

What would you like to do with this script?
> Display in terminal
  Save to file
  Run immediately
  Edit configuration
  Save as template
  Cancel
```

**Features:**
- Full control over every aspect of the script
- Interactive prompts for each option
- Visual configuration summary before generation
- Edit and regenerate as many times as needed
- Save successful configurations as templates

**📖 Learn more**: [Custom Script Configuration](custom-script.md)

---

### 6. Load Umbraco Versions Table

**Description**: Display a table of Umbraco CMS version numbers with their support status.

**What you see:**
```
Umbraco CMS Versions

┌─────────┬────────────────┬─────────────────┬──────────────────┐
│ Version │ Status         │ Release Date    │ Support Until    │
├─────────┼────────────────┼─────────────────┼──────────────────┤
│ 14.3.0  │ ✓ Latest       │ Dec 2024        │ Active           │
│ 14.2.0  │ ✓ Stable       │ Nov 2024        │ Active           │
│ 14.1.0  │ ✓ Stable       │ Oct 2024        │ Active           │
│ 14.0.0  │ ✓ LTS          │ May 2024        │ May 2027         │
│ 13.5.2  │ ✓ LTS          │ Apr 2024        │ Dec 2026         │
│ 13.4.1  │ ✓ LTS          │ Mar 2024        │ Dec 2026         │
│ 13.3.0  │ ✓ LTS          │ Feb 2024        │ Dec 2026         │
│ 13.2.0  │ ○ Maintenance  │ Jan 2024        │ Dec 2025         │
│ 13.1.0  │ ○ Maintenance  │ Dec 2023        │ Jun 2025         │
│ 13.0.0  │ ✗ End of Life  │ Jun 2023        │ Ended            │
│ 12.3.6  │ ✗ End of Life  │ May 2023        │ Ended            │
│ 10.8.7  │ ✗ End of Life  │ Dec 2022        │ Ended            │
└─────────┴────────────────┴─────────────────┴──────────────────┘

Legend:
  ✓ - Actively supported
  ○ - Maintenance mode
  ✗ - No longer supported

Press Enter to return to main menu...
```

**Features:**
- Shows all major Umbraco versions
- Support status and timelines
- Release dates
- LTS (Long Term Support) indicators
- Helps you choose the right version

**📖 Learn more**: [Umbraco Versions Reference](umbraco-versions.md)

---

### 7. Help

**Description**: Display comprehensive help information about the CLI tool.

**What you see:**
```
╔══════════════════════════════════════════╗
║    Package Script Writer Help            ║
╚══════════════════════════════════════════╝

USAGE:
  psw [options]

INTERACTIVE MODE:
  psw

  Launch the interactive menu-driven interface with guided
  prompts for generating Umbraco installation scripts.

CLI MODE:
  psw --default
  psw -p "uSync,Diplo.GodMode" -n MyProject
  psw -p "uSync|17.0.0" -n MyProject -s MySolution -u

TEMPLATE COMMANDS:
  psw template save <name> [options]
  psw template load <name> [options]
  psw template list
  psw template delete <name>

HISTORY COMMANDS:
  psw history list
  psw history show <#>
  psw history rerun <#>

COMMON OPTIONS:
  -h, --help              Show help information
  -v, --version           Show version number
  -p, --packages <pkgs>   Specify packages
  -n, --project-name <nm> Set project name
  -s, --solution <name>   Create solution
  -u, --unattended        Use unattended install
  --verbose               Enable detailed logging

For complete documentation, visit:
https://github.com/prjseal/Package-Script-Writer/blob/main/.github/cli-documentation.md

Press Enter to return to main menu...
```

**Features:**
- Quick reference for all commands
- Usage examples
- Link to full documentation
- Returns to main menu when done

---

## Script Actions Menu

After generating any script, you'll see this menu:

```
What would you like to do with this script?
(Press Ctrl+C at any time to start over)
> Run
  Edit
  Copy
  Save
  Start over
```

### Run
Executes the script immediately with command validation:

```
Enter directory to run script in [current directory]: ./projects/new-site

Validating script commands...
✓ All commands validated

Running script in: /home/user/projects/new-site

> dotnet new install Umbraco.Templates::14.3.0 --force
  ⠋ Installing...
  ✓ Template installed

> dotnet new umbraco --force -n "MyProject"
  ⠋ Creating project...
  ✓ Project created

[... execution continues ...]

✓ Script completed successfully!
```

### Edit
Returns you to the configuration editor to modify settings and regenerate the script.

### Copy
Copies the generated script to your clipboard for easy pasting.

```
✓ Script copied to clipboard
```

### Save
Saves the script to a file:

```
Enter file name [install-script.sh]: my-umbraco-setup.sh

✓ Script saved to my-umbraco-setup.sh
```

### Start over
Returns to the main menu without saving or running the script.

---

## Navigation Tips

### Keyboard Shortcuts

| Key | Action |
|-----|--------|
| **↑ / ↓** | Navigate menu options |
| **Space** | Select/deselect in multi-select lists |
| **Enter** | Confirm selection |
| **Ctrl+C** | Cancel current operation and return to main menu |
| **Esc** | Go back (in some prompts) |

### Multi-Select Lists

When you see checkboxes `[ ]`, you can:
- Use **Space** to toggle selection
- Use **↑/↓** to move between items
- Press **Enter** when done selecting

Example:
```
Select packages:
  [ ] Umbraco.Community.BlockPreview
  [X] Diplo.GodMode         ← Selected
  [X] uSync                 ← Selected
  [ ] Umbraco.Forms
```

### Single Select Lists

When you see an arrow `>`, only one item can be selected:
- Use **↑/↓** to change selection
- Press **Enter** to confirm

Example:
```
Select database type:
> SQLite        ← Currently selected
  LocalDb
  SQL Server
```

---

## Best Practices

### 1. Start with Default Script
If you're new to the tool, start with **"Use default script"** to see how it works.

### 2. Save Successful Configurations
When you create a configuration that works well, use **"Save as template"** to reuse it later.

### 3. Use History for Repeated Tasks
The history system automatically tracks your work. Use **"Load script from history"** to quickly repeat previous operations.

### 4. Explore Community Templates
Check out **"Use community template"** for pre-made configurations created by other users.

### 5. Enable Verbose Mode
If you encounter issues, restart with `psw --verbose` to see detailed diagnostic output.

---

## Troubleshooting

### Package List Not Loading
```
⚠ Warning: Could not load marketplace packages

Using cached package list from: ~/.psw/cache/packages-cache.json
```

**Solution**: Check your internet connection. The tool will use cached data if available.

### Template Not Found
```
✗ Error: Template 'my-template' not found

Available templates:
  - company-standard
  - blog-setup
```

**Solution**: Run `psw template list` to see available templates.

### Script Validation Failed
```
✗ Script validation failed - dangerous commands detected:

  • Line 5: Command not allowed: 'rm -rf /'

The script contains commands that are not in the allowlist.
```

**Solution**: The security system blocked a dangerous command. This is by design - only safe `dotnet` commands are allowed. See [Security Guide](security.md).

### History Empty
```
No history entries found.

Generate a script to start building your history.
```

**Solution**: History is only created when you generate scripts. Use any script generation option to create your first entry.

---

## Related Documentation

- **[CLI Documentation Hub](../cli-documentation.md)** - Main CLI documentation
- **[Templates Guide](templates.md)** - Template system documentation
- **[History Guide](history.md)** - History system documentation
- **[Security Guide](security.md)** - Security features
- **[Community Templates Guide](community-templates.md)** - Using community templates
- **[Custom Script Guide](custom-script.md)** - Building custom scripts
- **[Default Script Guide](default-script.md)** - Default script details
- **[Umbraco Versions Reference](umbraco-versions.md)** - Version information
- **[Process Flow Diagrams](../CLI-Interactive-Mode-Flow.md)** - Visual workflow diagrams

---

## Examples

### Quick Start Example
```bash
# 1. Start interactive mode
psw

# 2. Select "Use default script" from menu
# 3. Choose "Save to file"
# 4. Enter filename: quick-start.sh
# 5. Done! Script saved
```

### Custom Blog Setup Example
```bash
# 1. Start interactive mode
psw

# 2. Select "Create new script"
# 3. Select configuration options:
#    - Template & Project Settings
#    - Packages (select uSync, Diplo.GodMode)
#    - Starter Kit (select clean)
#    - Unattended Install (SQLite + admin credentials)
# 4. Review configuration summary
# 5. Generate script
# 6. Save as template: "my-blog-template"
# 7. Save script to file
```

### Using Templates Example
```bash
# 1. Start interactive mode
psw

# 2. Select "Use local template"
# 3. Choose "my-blog-template"
# 4. Select "Generate script with these settings"
# 5. Choose "Run immediately"
# 6. Enter directory: ./my-new-blog
# 7. Watch it execute!
```

---

<div align="center">

**📖 [Back to CLI Documentation](../cli-documentation.md)** | **🏠 [Main Repository](https://github.com/prjseal/Package-Script-Writer)**

Made with ❤️ for the Umbraco Community

</div>
