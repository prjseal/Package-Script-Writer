# Community Templates Guide

## Overview

Community templates are pre-made configuration templates created and shared by the Umbraco community. They provide quick-start configurations for common scenarios like blogs, e-commerce sites, portfolios, and more.

## What are Community Templates?

Community templates are YAML configuration files served via the PSW API at https://psw.codeshare.co.uk that define:
- Umbraco template package and version
- Pre-selected packages with specific versions
- Starter kit configuration
- Database and unattended install settings
- Docker configuration
- Project structure

They're perfect for:
- 🚀 **Quick starts** - Get up and running fast with proven configurations
- 📚 **Learning** - See how others configure their Umbraco projects
- 🎯 **Standardization** - Use the same setup across multiple projects
- 🤝 **Sharing** - Share your successful configurations with the community

## Using Community Templates

### In Interactive Mode

1. Start interactive mode:
   ```bash
   psw
   ```

2. Select **"Use community template"** from the main menu

3. Wait while templates are loaded from the API:
   ```
   ⠋ Loading community templates from PSW API...
   ```

4. Browse available templates:
   ```
   Select a community template:
   > Umbraco 14 Blog (with uSync & Forms)
     Umbraco 14 E-Commerce Starter
     Umbraco 14 Minimal Setup
     Umbraco 13 LTS Standard
     Custom Community Template URL...
   ```

5. Select a template to view details:
   ```
   Community Template: Umbraco 14 Blog
   Author: Community Contributors
   Description: Complete blog setup with uSync and Umbraco Forms

   Template includes:
     • Umbraco.Templates (latest)
     • uSync (17.0.0)
     • Umbraco.Forms (14.2.0)
     • Clean starter kit
     • SQLite database

   Would you like to:
   > Use this template
     Edit configuration first
     View template source
     Cancel
   ```

6. Choose an action:
   - **Use this template** - Generate script immediately with these settings
   - **Edit configuration first** - Modify settings before generating
   - **View template source** - See the raw YAML file
   - **Cancel** - Return to main menu

### Using a Custom Template URL

If you have a direct link to a community template:

1. Select **"Custom Community Template URL..."**

2. Enter the URL:
   ```
   Enter template URL (GitHub raw URL or direct YAML link):
   https://raw.githubusercontent.com/username/repo/main/templates/my-template.yaml
   ```

3. The template will be downloaded and loaded

## Template Repository

Community templates are served via the PSW API and stored in the website deployment:

🌐 **API Endpoint**: https://psw.codeshare.co.uk/api/communitytemplates

📦 **Source Repository**: [prjseal/Package-Script-Writer](https://github.com/prjseal/Package-Script-Writer)

📁 **GitHub Location**: `/src/PSW/community-templates/` directory

Templates are fetched from the API (not directly from GitHub), ensuring reliability and caching support.

## Available Templates

### Umbraco 14 Blog
**File**: `umbraco-14-blog.yaml`
**Description**: Complete blog setup with content management features

**Includes:**
- Umbraco.Templates (latest stable)
- uSync 17.0.0 (content sync)
- Umbraco.Forms 14.2.0 (form builder)
- Diplo.GodMode (developer tools)
- Clean starter kit
- SQLite database (for development)

**Best for**: Blogs, news sites, content-heavy websites

### Umbraco 14 E-Commerce Starter
**File**: `umbraco-14-ecommerce.yaml`
**Description**: E-commerce ready setup with product management

**Includes:**
- Umbraco.Templates (latest stable)
- Umbraco.Commerce (latest)
- uSync (for deployment)
- SQL Server database configuration
- Docker support (Dockerfile + Docker Compose)

**Best for**: Online stores, product catalogs, commercial websites

### Umbraco 14 Minimal Setup
**File**: `umbraco-14-minimal.yaml`
**Description**: Bare-bones Umbraco installation

**Includes:**
- Umbraco.Templates (latest stable)
- No additional packages
- No starter kit
- SQLite database

**Best for**: Starting from scratch, custom builds, learning

### Umbraco 13 LTS Standard
**File**: `umbraco-13-lts-standard.yaml`
**Description**: Long-term support version with common packages

**Includes:**
- Umbraco.Templates 13.x LTS
- uSync (LTS compatible version)
- Umbraco.Forms (LTS compatible)
- Clean starter kit
- LocalDb database

**Best for**: Enterprise projects requiring long-term support

## Creating Community Templates

Want to contribute your own template? Here's how:

### 1. Create the Template File

Create a YAML file following this structure:

```yaml
metadata:
  name: my-awesome-template
  description: Brief description of what this template provides
  author: YourGitHubUsername
  created: 2025-12-18T00:00:00Z
  version: 1.0.0
  tags:
    - umbraco14
    - blog
    - beginner-friendly

configuration:
  template:
    name: Umbraco.Templates
    version: ""  # Latest stable

  project:
    name: MyProject
    createSolution: true
    solutionName: MySolution

  packages:
    - name: uSync
      version: 17.0.0
    - name: Diplo.GodMode
      version: 3.0.3

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
      email: admin@example.com
      password: <prompt>  # Use <prompt> for security

  output:
    oneliner: false
    removeComments: false
    includePrerelease: false

  execution:
    autoRun: false
    runDirectory: .
```

### 2. Test Your Template

Test locally before contributing:

```bash
# Import your template
psw template import my-awesome-template.yaml

# Test loading it
psw template load my-awesome-template

# Generate and verify the script works
```

### 3. Submit to Community

1. Fork the [Package Script Writer repository](https://github.com/prjseal/Package-Script-Writer)

2. Add your template file to `/src/PSW/community-templates/`

3. Update `/src/PSW/community-templates/index.json` with your template info:
   ```json
   {
     "name": "my-awesome-template",
     "displayName": "My Awesome Template",
     "description": "Brief description of what this template does",
     "author": "Your Name",
     "tags": ["blog", "umbraco14"],
     "fileName": "my-awesome-template.yaml",
     "created": "2024-12-19"
   }
   ```

4. Create a pull request with:
   - Your template file
   - Updated index.json
   - Brief description of what it provides

**Note:** Templates are stored in `/src/PSW/community-templates/` so they can be deployed with the website and served via the API.

### Template Guidelines

**DO:**
- ✅ Use descriptive names and descriptions
- ✅ Include version numbers for packages
- ✅ Use `<prompt>` for passwords (security)
- ✅ Test your template before submitting
- ✅ Document what packages are included and why
- ✅ Target specific use cases

**DON'T:**
- ❌ Include hardcoded passwords
- ❌ Use deprecated packages
- ❌ Create overly complex templates (keep it simple)
- ❌ Duplicate existing templates without good reason

## Template Categories

Templates are organized by category:

| Category | Purpose |
|----------|---------|
| **Blog** | Content-heavy sites, news, articles |
| **E-Commerce** | Online stores, product catalogs |
| **Minimal** | Bare-bones starting points |
| **Enterprise** | Large-scale, LTS versions |
| **Developer** | Developer tools, debugging |
| **Marketing** | Landing pages, campaigns |
| **Portfolio** | Personal sites, showcases |

## Security Considerations

### Password Handling

Community templates should **never** include hardcoded passwords:

```yaml
# ✅ CORRECT - Prompts user at runtime
password: <prompt>

# ❌ WRONG - Security risk
password: "MyPassword123"
```

### Environment Variables

Templates can reference environment variables:

```yaml
# ✅ Good for sensitive data
password: ${ADMIN_PASSWORD}
connectionString: ${DATABASE_CONNECTION}
```

Users must set these before running:
```bash
export ADMIN_PASSWORD="SecurePass123!"
psw template load my-template
```

### Connection Strings

For database connection strings:

```yaml
# ✅ For SQLite (safe default)
connectionString: null

# ✅ For SQL Server (uses environment variable)
connectionString: ${SQL_CONNECTION_STRING}

# ❌ Don't hardcode real credentials
connectionString: "Server=prod-db;User=admin;Password=secret123"
```

## Caching

Community templates are cached locally after first download:

**Cache location**: `~/.psw/cache/community-templates/`

**Refresh cache**:
```bash
# Clear all caches
psw --clear-cache

# Templates will be re-downloaded on next use
```

**Cache duration**: 24 hours (automatically refreshed)

## Troubleshooting

### Template Not Loading

**Problem:**
```
⚠ Warning: Could not load community templates from PSW API

Using cached templates from: ~/.psw/cache/community-templates/
```

**Solutions:**
1. Check internet connection
2. Verify https://psw.codeshare.co.uk is accessible
3. Try clearing cache: `psw --clear-cache`
4. Use custom URL to load template directly

### Invalid Template Format

**Problem:**
```
✗ Error: Invalid template format

Template validation failed:
  • Missing required field: configuration.template.name
  • Invalid package version format: "latest-stable"
```

**Solution:**
- Template author needs to fix the YAML format
- Report the issue on the template's repository
- Use a different template or create your own

### Package Not Found

**Problem:**
```
⚠ Warning: Package 'OldPackageName' not found on NuGet

This package may have been renamed or removed.
```

**Solution:**
- Template may be outdated
- Check if package was renamed
- Edit template to use current package name
- Report to template author

### Version Conflict

**Problem:**
```
⚠ Warning: Package version conflict

Template specifies uSync 15.0.0 but this is not compatible with
Umbraco 14. Latest compatible version is 17.0.0.
```

**Solution:**
- Select "Edit configuration first" when loading
- Update package versions to compatible ones
- Report incompatibility to template author

## Versus Local Templates

| Feature | Community Templates | Local Templates |
|---------|---------------------|-----------------|
| **Source** | PSW API (https://psw.codeshare.co.uk) | Your local machine |
| **Sharing** | Public, discoverable | Manual export/import |
| **Updates** | Auto-updated from API | Manual updates |
| **Caching** | 1-hour cache | Permanent until deleted |
| **Creation** | Pull request required | Create anytime |
| **Modification** | Fork and PR | Edit directly |

**Best Practice**: Use community templates as starting points, then save customized versions as local templates.

## Examples

### Example 1: Quick Blog Setup

```bash
# Start interactive mode
psw

# Select "Use community template"
# Choose "Umbraco 14 Blog"
# Select "Use this template"
# Choose "Save to file"
# Enter filename: blog-setup.sh
# Done! Script ready to run
```

### Example 2: Customizing a Community Template

```bash
# Start interactive mode
psw

# Select "Use community template"
# Choose "Umbraco 14 E-Commerce Starter"
# Select "Edit configuration first"
# Modify:
#   - Change project name to "MyStore"
#   - Change database to SQL Server
#   - Add custom packages
# Generate script
# Save as local template: "my-store-template"
```

### Example 3: Using Custom URL

```bash
# Start interactive mode
psw

# Select "Use community template"
# Choose "Custom Community Template URL..."
# Enter: https://gist.githubusercontent.com/user/hash/raw/template.yaml
# Template loads and shows details
# Select "Use this template"
```

## Contributing

We welcome community template contributions!

**Steps to contribute:**

1. Create and test your template
2. Fork the repository
3. Add your template to `/src/PSW/community-templates/`
4. Update `/src/PSW/community-templates/index.json`
5. Submit a pull request
6. Respond to review feedback
7. Once merged, templates are automatically available via the API after deployment

**Review criteria:**
- Template works as described
- No hardcoded passwords or secrets
- Packages are current and compatible
- Good documentation in metadata
- Follows YAML format standards

**Note:** Templates are deployed with the website at https://psw.codeshare.co.uk and served via API endpoints for reliability and performance.

## Related Documentation

- **[Templates Guide](templates.md)** - Local template system
- **[Interactive Mode](interactive-mode.md)** - Using interactive mode
- **[Custom Script Guide](custom-script.md)** - Building custom scripts from scratch
- **[Security Guide](security.md)** - Security best practices

---

<div align="center">

**🏠 [Back to Interactive Mode](interactive-mode.md)** | **📖 [CLI Documentation](../cli-documentation.md)**

</div>
