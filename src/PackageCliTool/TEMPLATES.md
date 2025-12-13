# Template System Documentation

## Overview

The template system allows you to save, reuse, and share CLI configurations for the Package Script Writer tool. This eliminates repetitive typing and ensures consistency across projects.

## Quick Start

### Save a Template

```bash
# Save configuration from command-line flags
psw template save my-blog \
    -p "uSync|17.0.0,Umbraco.Forms|14.2.0" \
    -n MyBlog \
    --template-description "Standard blog setup" \
    --template-tags "blog,umbraco14"
```

### Load a Template

```bash
# Load and execute a template
psw template load my-blog

# Load with overrides
psw template load my-blog -n NewBlog --auto-run
```

### List Templates

```bash
# List all available templates
psw template list
```

## Template Commands

### `template save <name>`

Saves the current configuration as a reusable template.

**Options:**
- `--template-description <desc>` - Human-readable description
- `--template-tags <tags>` - Comma-separated tags for categorization
- All standard CLI options (packages, project settings, template package, etc.)

**Example:**
```bash
psw template save company-standard \
    --template-package Umbraco.Templates \
    -t "14.3.0" \
    -p "uSync|17.0.0,Umbraco.Forms|14.2.0,clean|7.0.1" \
    -n MyProject \
    -s MySolution \
    -u --database-type SQLite \
    --admin-email "admin@example.com" \
    --template-description "Company standard Umbraco setup" \
    --template-tags "company,standard,umbraco14"
```

### `template load <name>`

Loads and executes a saved template.

**Options:**
- `-n, --project-name <name>` - Override project name
- `--run-dir <dir>` - Override run directory
- `--auto-run` - Automatically execute the script
- `--set <key=value>` - Override specific values

**Examples:**
```bash
# Basic load
psw template load my-blog

# Load with project name override
psw template load my-blog -n ClientBlog

# Load with multiple overrides
psw template load my-blog \
    -n ClientBlog \
    --set DatabaseType=SQLServer \
    --set AutoRun=true \
    --auto-run
```

### `template list`

Lists all available templates.

**Example:**
```bash
psw template list

# Output:
# ┌────────────────────┬─────────────────────────────┬──────────┬────────────────┐
# │ Name               │ Description                 │ Version  │ Tags           │
# ├────────────────────┼─────────────────────────────┼──────────┼────────────────┤
# │ my-blog            │ Standard blog setup         │ 1.0.0    │ blog, umbraco14│
# │ company-standard   │ Company standard setup      │ 1.0.0    │ company        │
# └────────────────────┴─────────────────────────────┴──────────┴────────────────┘
```

### `template show <name>`

Displays detailed information about a template.

**Example:**
```bash
psw template show my-blog

# Shows:
# - Metadata (name, description, author, version, tags, dates)
# - Configuration (template, project, packages, options)
```

### `template delete <name>`

Deletes a template.

**Example:**
```bash
psw template delete old-template

# Prompts for confirmation before deleting
```

### `template export <name>`

Exports a template to a YAML file.

**Options:**
- `--template-file <path>` - Output file path (default: `<name>.yaml`)

**Examples:**
```bash
# Export to default location
psw template export my-blog

# Export to specific file
psw template export my-blog --template-file /path/to/my-blog.yaml
```

### `template import <file>`

Imports a template from a YAML file.

**Options:**
- `--template-name <name>` - Optionally rename the template

**Examples:**
```bash
# Import with original name
psw template import my-blog.yaml

# Import with new name
psw template import my-blog.yaml --template-name new-blog
```

### `template validate <file>`

Validates a template file without importing it.

**Example:**
```bash
psw template validate my-blog.yaml

# Output:
# ✓ Template is valid
# OR
# ⚠ Template validation warnings:
#   • Project name: Invalid characters
#   • Package 'OldPackage' version: Invalid format
```

## Template File Format

Templates are stored as YAML files in `~/.psw/templates/`.

### Complete Example File

`~/.psw/templates/my-blog.yaml`:

```yaml
metadata:
  name: my-blog
  description: Standard blog setup with uSync and Forms
  author: john.doe
  created: 2025-12-13T15:30:42.1234567Z
  modified: 2025-12-13T15:30:42.1234567Z
  version: 1.0.0
  tags:
  - blog
  - umbraco14

configuration:
  template:
    name: Umbraco.Templates
    version: ""

  project:
    name: MyBlog
    createSolution: true
    solutionName: MyBlog

  packages:
  - name: uSync
    version: 13.2.1
  - name: Umbraco.Community.BlockPreview
    version: 13.0.0

  starterKit:
    enabled: true
    package: clean

  docker:
    dockerfile: false
    dockerCompose: false

  unattended:
    enabled: true
    database:
      type: SQLite
      connectionString: null
    admin:
      name: Administrator
      email: admin@myblog.com
      password: SecurePass123!

  output:
    oneliner: false
    removeComments: false
    includePrerelease: false

  execution:
    autoRun: false
    runDirectory: .
```

### Template Structure

| Section | Description |
|---------|-------------|
| **metadata** | Template information (name, author, tags, etc.) |
| **configuration** | Script generation settings (organized into subsections) |
| └─ **template** | Umbraco template package name and version |
| └─ **project** | Project and solution settings |
| └─ **packages** | List of NuGet packages to install |
| └─ **starterKit** | Starter kit configuration |
| └─ **docker** | Docker setup options |
| └─ **unattended** | Unattended install settings (DB, admin) |
| └─ **output** | Script formatting options |
| └─ **execution** | Auto-run and directory settings |

### Field Details

#### Metadata Section

```yaml
metadata:
  name: my-template           # Required: Template identifier
  description: "..."          # Recommended: What this template does
  author: username            # Auto-filled: Creator username
  created: 2025-12-13T...     # Auto-filled: Creation timestamp
  modified: 2025-12-13T...    # Auto-updated: Last modification
  version: 1.0.0              # Template version (semver)
  tags: [tag1, tag2]          # Optional: For categorization
```

#### Configuration.Template Section

```yaml
template:
  name: Umbraco.Templates      # Template package name
  version: "14.3.0"            # Specific version or "" for latest
```

#### Configuration.Project Section

```yaml
project:
  name: MyProject              # Project name
  createSolution: true         # Create .sln file?
  solutionName: MySolution     # Solution name (if creating)
```

#### Configuration.Packages Section

```yaml
packages:                      # List of NuGet packages
- name: uSync
  version: 13.2.1              # Specific version
- name: Umbraco.Forms
  version: latest              # Latest stable
- name: MyPackage
  version: prerelease          # Latest prerelease
```

#### Configuration.StarterKit Section

```yaml
starterKit:
  enabled: true                # Include starter kit?
  package: clean               # Package name (or with version)
                               # e.g., "clean --version 7.0.1"
```

#### Configuration.Docker Section

```yaml
docker:
  dockerfile: false            # Create Dockerfile?
  dockerCompose: false         # Create docker-compose.yml?
```

#### Configuration.Unattended Section

```yaml
unattended:
  enabled: true                # Use unattended install?
  database:
    type: SQLite               # SQLite, LocalDb, SQLServer, etc.
    connectionString: null     # Required for SQLServer/SQLAzure
  admin:
    name: Administrator        # Admin user name
    email: admin@example.com   # Admin email
    password: <prompt>         # Password (see security options)
```

#### Configuration.Output Section

```yaml
output:
  oneliner: false              # Output as single line?
  removeComments: false        # Remove script comments?
  includePrerelease: false     # Include prerelease packages?
```

#### Configuration.Execution Section

```yaml
execution:
  autoRun: false               # Auto-execute after generation?
  runDirectory: .              # Directory to run in
```

## Advanced Features

### Password Handling

Templates support three ways to handle passwords:

1. **Prompt at runtime** (recommended):
   ```yaml
   password: <prompt>
   ```

2. **Environment variable**:
   ```yaml
   password: ${UMBRACO_ADMIN_PASS}
   ```

3. **Literal** (not recommended for security):
   ```yaml
   password: "MyPassword123"
   ```

### Overrides

You can override specific template values when loading:

```bash
# Override project name
psw template load my-blog -n NewBlog

# Override multiple values
psw template load my-blog \
    --set ProjectName=NewBlog \
    --set DatabaseType=SQLServer \
    --set AutoRun=true
```

### Package Version Formats

Templates support flexible package version specifications:

```yaml
packages:
  - name: uSync
    version: "17.0.0"      # Specific version

  - name: Umbraco.Forms
    version: latest         # Latest stable

  - name: Diplo.GodMode
    version: prerelease     # Latest prerelease
```

## Use Cases

### 1. Personal Development Workflow

Save your preferred setup once and reuse it:

```bash
# Save once
psw template save my-standard -p "uSync|17.0.0" -n MyProject -u --database-type SQLite

# Reuse many times
psw template load my-standard -n Project1
psw template load my-standard -n Project2
psw template load my-standard -n Project3
```

### 2. Team Standardization

Share templates across your team via Git:

```bash
# Team lead creates and exports template
psw template save company-standard [options]
psw template export company-standard --template-file team-templates/company-standard.yaml

# Commit to Git
git add team-templates/company-standard.yaml
git commit -m "Add company standard template"
git push

# Team members import
git pull
psw template import team-templates/company-standard.yaml
```

### 3. Client Project Variants

Create templates for different client tiers:

```bash
# Basic tier
psw template save client-basic -p "uSync" -u --database-type SQLite

# Premium tier
psw template save client-premium -p "uSync,Umbraco.Forms" -u --database-type SQLServer

# Enterprise tier
psw template save client-enterprise -p "uSync,Umbraco.Forms,Umbraco.Deploy" --dockerfile --docker-compose
```

### 4. CI/CD Pipelines

Use templates in automated workflows:

```yaml
# .github/workflows/setup.yml
- name: Setup Umbraco
  run: |
    psw template import .psw/production.yaml
    psw template load production --auto-run --run-dir ./deploy
```

## Storage Location

Templates are stored in your home directory:

```
~/.psw/
├── templates/
│   ├── my-blog.yaml
│   ├── company-standard.yaml
│   └── client-premium.yaml
├── cache/
└── config.json
```

## Best Practices

1. **Use descriptive names**: `blog-umbraco14-sqlite` is better than `template1`

2. **Add tags**: Help categorize and filter templates
   ```bash
   --template-tags "blog,umbraco14,sqlite,production"
   ```

3. **Use `<prompt>` for passwords**: Never commit passwords to templates
   ```yaml
   password: <prompt>
   ```

4. **Document in description**: Explain what the template is for
   ```bash
   --template-description "Standard blog setup with uSync and Forms for Umbraco 14"
   ```

5. **Version your templates**: Export to files and commit to Git for team sharing

6. **Validate before importing**: Check templates are valid
   ```bash
   psw template validate team-templates/new-template.yaml
   psw template import team-templates/new-template.yaml
   ```

## Troubleshooting

### Template not found

```bash
# List all templates to see available names
psw template list

# Ensure the template exists
ls ~/.psw/templates/
```

### Template validation errors

```bash
# Validate the template to see specific errors
psw template validate my-template.yaml

# Common issues:
# - Invalid package names
# - Invalid version formats
# - Invalid project/solution names
# - Missing required fields
```

### Environment variable not found

If using `${VAR_NAME}` in templates:

```bash
# Set the environment variable before loading
export UMBRACO_ADMIN_PASS="MySecurePassword"
psw template load my-template
```

### Override not working

Ensure you're using the correct override key names:

```bash
# Correct
psw template load my-blog --set ProjectName=NewBlog

# Incorrect (wrong case)
psw template load my-blog --set projectname=NewBlog
```

Valid override keys:
- `ProjectName`
- `SolutionName`
- `DatabaseType`
- `AutoRun`
- `RunDirectory`

## Examples

### Complete Workflow Example

```bash
# 1. Create a template from command-line flags
psw template save my-umbraco-blog \
    --template-package Umbraco.Templates \
    -t "14.3.0" \
    -p "uSync|17.0.0,Umbraco.Forms|14.2.0,clean|7.0.1" \
    -n MyBlog \
    -s MyBlog \
    -k --starter-kit clean \
    -u --database-type SQLite \
    --admin-email "admin@myblog.com" \
    --template-description "Standard Umbraco 14 blog with uSync and Forms" \
    --template-tags "blog,umbraco14,sqlite"

# 2. List templates to verify
psw template list

# 3. View template details
psw template show my-umbraco-blog

# 4. Export for sharing
psw template export my-umbraco-blog --template-file my-blog.yaml

# 5. Share with team (commit to Git)
git add my-blog.yaml
git commit -m "Add blog template"
git push

# 6. Team member imports
git pull
psw template import my-blog.yaml

# 7. Use the template for a new project
psw template load my-umbraco-blog -n ClientBlog --auto-run

# 8. Clean up old templates
psw template delete old-template
```

## Migration Guide

### From Manual CLI Flags to Templates

Before (repetitive):
```bash
psw -p "uSync|17.0.0,Umbraco.Forms|14.2.0" -n Project1 -u --database-type SQLite --admin-email admin@p1.com --admin-password Pass123!
psw -p "uSync|17.0.0,Umbraco.Forms|14.2.0" -n Project2 -u --database-type SQLite --admin-email admin@p2.com --admin-password Pass123!
psw -p "uSync|17.0.0,Umbraco.Forms|14.2.0" -n Project3 -u --database-type SQLite --admin-email admin@p3.com --admin-password Pass123!
```

After (with template):
```bash
# Save once
psw template save standard-setup -p "uSync|17.0.0,Umbraco.Forms|14.2.0" -u --database-type SQLite

# Reuse
psw template load standard-setup -n Project1
psw template load standard-setup -n Project2
psw template load standard-setup -n Project3
```

## See Also

- [Main README](README.md) - General CLI documentation
- [API Reference](../../.github/api-reference.md) - API endpoints documentation
