# History System Documentation

## Overview

The history system automatically tracks all script generations, allowing you to view, re-run, and analyze your past work. This is useful for auditing, debugging, and quickly repeating previous operations.

## Quick Start

### View Recent History

```bash
# List the 10 most recent scripts
psw history list

# List more entries
psw history list --history-limit 25
```

### Re-run a Previous Script

```bash
# Re-run entry #3
psw history rerun 3
```

### View Details

```bash
# Show full details of entry #1
psw history show 1
```

## History Commands

### `history list`

Lists recent script generation history.

**Options:**
- `--history-limit <count>` - Number of entries to show (default: 10)

**Example:**
```bash
# List 10 most recent entries
psw history list

# List 50 entries
psw history list --history-limit 50

# Output:
# ┌───┬─────────────────┬────────────┬──────────┬────────────┬─────────────┐
# │ # │ Date/Time       │ Project    │ Template │ Status     │ Description │
# ├───┼─────────────────┼────────────┼──────────┼────────────┼─────────────┤
# │ 1 │ 2025-01-15 14:30│ MyBlog     │ my-blog  │ ✓ Executed │ Blog setup  │
# │ 2 │ 2025-01-15 10:15│ ClientSite │ None     │ Not exec.  │ -           │
# │ 3 │ 2025-01-14 16:45│ TestProj   │ standard │ ✗ Failed(1)│ Test run    │
# └───┴─────────────────┴────────────┴──────────┴────────────┴─────────────┘
```

### `history show <#>`

Shows detailed information about a specific history entry.

**Arguments:**
- `<#>` - Entry number from the list (required)

**Example:**
```bash
psw history show 3

# Shows:
# - Entry metadata (ID, timestamp, project, template, tags)
# - Execution status and exit code
# - Full configuration used
# - Script preview
```

### `history rerun <#>`

Regenerates and optionally re-executes a script from history.

**Arguments:**
- `<#>` - Entry number from the list (required)

**Workflow:**
1. Shows what will be re-run
2. Confirms with user
3. Regenerates the script using the same configuration
4. Displays the new script
5. Optionally executes it
6. Saves the new execution as a new history entry

**Example:**
```bash
psw history rerun 1

# Interactive prompts:
# Re-running: MyBlog - 2025-01-15 14:30
# Regenerate and execute this script? (y/n): y
# ⭐ Generating installation script...
# ✓ Script regenerated
# Execute the script? (y/n): y
# Enter directory path: ./projects/myblog
# ... script executes ...
```

### `history delete <#>`

Deletes a specific history entry.

**Arguments:**
- `<#>` - Entry number from the list (required)

**Example:**
```bash
psw history delete 5

# Prompts for confirmation before deleting
```

### `history clear`

Clears all history entries.

**Example:**
```bash
psw history clear

# Output:
# Delete all 42 history entries? This cannot be undone. (y/n): y
# ✓ Cleared 42 history entries
```

## History File Format

### Complete Example File

After generating one script, `~/.psw/history/history.yaml` looks like this:

```yaml
version: 1.0.0
maxEntries: 50
entries:
- id: 8a3f5c21-9d42-4e7b-b1a3-c8e6f4d2a9b7
  timestamp: 2025-12-13T15:30:42.1234567Z
  scriptModel:
    templateName: Umbraco.Templates
    templateVersion: ""
    projectName: MyBlog
    createSolutionFile: true
    solutionName: MyBlog
    packages: uSync|13.2.1,Umbraco.Community.BlockPreview|13.0.0
    includeStarterKit: true
    starterKitPackage: clean
    includeDockerfile: false
    includeDockerCompose: false
    canIncludeDocker: false
    useUnattendedInstall: true
    databaseType: SQLite
    connectionString: null
    userFriendlyName: Administrator
    userEmail: admin@myblog.com
    userPassword: SecurePass123!
    onelinerOutput: false
    removeComments: false
  templateName: null
  wasExecuted: false
  executionDirectory: null
  exitCode: null
  description: Script for MyBlog
  tags: []
```

### After Execution

When the script is executed, the entry updates:

```yaml
version: 1.0.0
maxEntries: 50
entries:
- id: 8a3f5c21-9d42-4e7b-b1a3-c8e6f4d2a9b7
  timestamp: 2025-12-13T15:30:42.1234567Z
  scriptModel:
    # ... (same as above)
  templateName: null
  wasExecuted: true                          # ← Updated
  executionDirectory: /home/user/projects/myblog  # ← Added
  exitCode: 0                                # ← Added
  description: Script for MyBlog
  tags: []
```

**Note:** Script content is **not** stored in history for security reasons. Scripts are regenerated from the configuration when you re-run a history entry.

### Multiple Entries

After generating 3 scripts, entries are in **reverse chronological order** (newest first):

```yaml
version: 1.0.0
maxEntries: 50
entries:
- id: c4d7e8f1-2a5b-4c9e-a3d6-f7e8c9d0a1b2  # Script #1 (most recent)
  timestamp: 2025-12-13T16:45:12.7890123Z
  scriptModel:
    projectName: EcommerceSite
    # ... config ...
  description: Script for EcommerceSite
  # ...

- id: b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e  # Script #2
  timestamp: 2025-12-13T16:15:30.4567890Z
  scriptModel:
    projectName: PortfolioSite
    # ... config ...
  description: Script for PortfolioSite
  # ...

- id: 8a3f5c21-9d42-4e7b-b1a3-c8e6f4d2a9b7  # Script #3 (oldest)
  timestamp: 2025-12-13T15:30:42.1234567Z
  scriptModel:
    projectName: MyBlog
    # ... config ...
  description: Script for MyBlog
  # ...
```

### History Entry Structure

Each entry contains:

| Field | Type | Description |
|-------|------|-------------|
| `id` | string (GUID) | Unique identifier |
| `timestamp` | DateTime (UTC) | When script was generated |
| `scriptModel` | object | Complete ScriptModel (flat structure) |
| `templateName` | string? | Template name if from template |
| `wasExecuted` | boolean | Whether script was run |
| `executionDirectory` | string? | Where it was executed |
| `exitCode` | int? | Exit code (0 = success) |
| `description` | string? | User description |
| `tags` | array | Tags for categorization |

**Security Note:** Script content is **not** stored in history files. When you re-run a history entry, the script is regenerated from the stored configuration. This prevents malicious script injection via modified history files.

### ScriptModel Format

The `scriptModel` field uses the internal API format (flat structure with string-based packages):

```yaml
scriptModel:
  # Template info
  templateName: Umbraco.Templates
  templateVersion: "14.3.0"

  # Project info
  projectName: MyBlog
  createSolutionFile: true
  solutionName: MyBlog

  # Packages (comma-separated string with | for versions)
  packages: "uSync|13.2.1,Umbraco.Forms|14.2.0,clean"

  # Starter kit
  includeStarterKit: true
  starterKitPackage: clean

  # Docker
  includeDockerfile: false
  includeDockerCompose: false
  canIncludeDocker: false

  # Unattended install
  useUnattendedInstall: true
  databaseType: SQLite
  connectionString: null
  userFriendlyName: Administrator
  userEmail: admin@myblog.com
  userPassword: SecurePass123!

  # Output options
  onelinerOutput: false
  removeComments: false
```

## Storage Location

History is stored in your home directory:

```
~/.psw/
├── history/
│   └── history.yaml    # All history entries (single file)
├── templates/
│   ├── template1.yaml  # Individual template files
│   └── template2.yaml
└── config.json
```

## History vs Templates

### Key Differences

| Aspect | History | Templates |
|--------|---------|-----------|
| **Purpose** | Automatic record of generated scripts | Reusable configuration blueprints |
| **Creation** | Automatic (every script generation) | Manual (explicitly saved) |
| **Storage** | Single file (`history.yaml`) | One file per template (`*.yaml`) |
| **Contains Script** | ❌ No - regenerates on re-run (security) | ❌ No - regenerates on load |
| **Data Format** | ScriptModel (flat, internal API format) | TemplateConfiguration (nested, organized) |
| **Packages Format** | String: `"pkg1\|ver1,pkg2\|ver2"` | List: `[{name, version}, ...]` |
| **Metadata** | Minimal (ID, timestamp, description) | Rich (name, author, version, tags) |
| **Execution Tracking** | ✅ Yes (wasExecuted, exitCode, directory) | ❌ No |
| **Modification** | Read-only (view, re-run, delete) | Editable (save, export, import) |
| **Sharing** | Not designed for sharing | Designed for team sharing |
| **Security** | ✅ Script regenerated (prevents tampering) | ✅ Config only (no script injection) |

### When to Use Each

**Use History When:**
- Tracking what scripts you've generated
- Re-running an exact script
- Auditing past work
- Viewing execution statistics

**Use local templates When:**
- Reusing configurations across projects
- Creating similar projects repeatedly
- Sharing configs with your team
- Standardizing setups

### Example Comparison

**Same Configuration, Different Format:**

```yaml
# History Entry (ScriptModel - flat structure)
scriptModel:
  templateName: Umbraco.Templates
  projectName: MyBlog
  packages: "uSync|13.2.1,Umbraco.Forms|14.2.0"  # String
  useUnattendedInstall: true
  databaseType: SQLite

# Template (TemplateConfiguration - nested structure)
configuration:
  template:
    name: Umbraco.Templates
  project:
    name: MyBlog
  packages:                                        # List
    - name: uSync
      version: 13.2.1
    - name: Umbraco.Forms
      version: 14.2.0
  unattended:
    enabled: true
    database:
      type: SQLite
```

### Workflow Integration

History and templates work together:

```bash
# 1. Use template to generate script
psw template load my-blog -n Project1
# → Creates history entry automatically

# 2. View what was generated
psw history list
# → Shows the generation from template

# 3. Re-run from history (faster than reloading template)
psw history rerun 1
# → Re-runs exact same script

# 4. Or use local template again for new project
psw template load my-blog -n Project2
# → Creates another history entry
```

## Automatic History Tracking

History is **automatically created** whenever you generate a script using:
- Interactive mode
- CLI mode
- Template loading

You don't need to do anything special - just use the tool normally and history is tracked.

## History Limits

### Default Limits

- **Max Entries**: 50 entries (oldest are automatically removed)
- **List Display**: 10 entries by default (use `--history-limit` for more)

### Customizing Limits

The max entries limit is configurable in the history file:

```yaml
# ~/.psw/history/history.yaml
version: "1.0.0"
maxEntries: 100  # Increase to keep more history
entries:
  - id: "..."
    # ...
```

## Use Cases

### 1. **Auditing Script Generations**

Track when and what scripts were generated:

```bash
# See all recent activity
psw history list --history-limit 50

# View details of a specific generation
psw history show 5
```

### 2. **Quick Re-execution**

Quickly re-run a previous script without re-entering parameters:

```bash
# List recent scripts
psw history list

# Re-run the most recent
psw history rerun 1
```

### 3. **Troubleshooting Failed Executions**

Find and analyze scripts that failed:

```bash
# List history to find failed entries (marked in red)
psw history list

# View details of the failed entry
psw history show 3
```

### 4. **Comparing Configurations**

Compare different script configurations:

```bash
# Show details of multiple entries
psw history show 1
psw history show 2
psw history show 3

# Compare packages, settings, etc.
```

### 5. **Learning from Past Work**

Review past configurations to remember what worked:

```bash
# List history
psw history list

# Re-use the same configuration
psw history rerun 5
```

## Advanced Features

### Entry Identification

Entries can be referenced by:
- **Number** (position in list): `psw history rerun 1`

### Automatic Description Generation

If no description is provided, entries automatically get a descriptive name based on:
1. User-provided description (if set)
2. Template name (if from template)
3. Project name + timestamp

### Tags Support

History entries support tags for categorization (though currently set programmatically):

```yaml
tags:
  - production
  - client-project
  - blog
```

Future versions may allow filtering by tags.

### Execution Tracking

When you execute a script, the history automatically records:
- That it was executed
- The directory it ran in
- The exit code (success/failure)

## Best Practices

### 1. **Regular Cleanup**

Periodically clean old history:

```bash
# Review old entries
psw history list --history-limit 50

# Delete specific unwanted entries
psw history delete 45
psw history delete 47

# Or clear everything if needed
psw history clear
```

### 2. **Use with Templates**

Combine history with templates for maximum efficiency:

```bash
# Load template (creates history entry)
psw template load my-blog -n Project1

# Later, re-run the exact same thing
psw history rerun 1
```

## Troubleshooting

### History not showing entries

**Cause**: History is only created when you **generate scripts**, not just when you Use local template/history commands.

**Solution**: Generate a script using any method:
```bash
psw --default          # Generates default script
psw template load my-blog  # Loads and generates from template
```

### Entry not found

```bash
# List all entries to see valid numbers
psw history list
```

### Too many entries in list

```bash
# Limit the display
psw history list --history-limit 5

# Or clear old entries
psw history clear
```

### History file corrupted

**Cause**: Manual edits or disk issues

**Solution**:
```bash
# Backup the file
cp ~/.psw/history/history.yaml ~/.psw/history/history.yaml.bak

# Clear and start fresh
psw history clear

# If needed, manually fix the YAML file and restore
```

## Examples

### Complete Workflow

```bash
# 1. Generate a script (creates history entry #1)
psw template load my-blog -n ClientBlog

# 2. List history
psw history list
# Shows: #1 | 2025-01-15 14:30 | ClientBlog | my-blog | ✓ Executed

# 3. Re-run for another client
psw history rerun 1
# Regenerates the same script, execute for different directory

# 4. Clean up old test entries
psw history delete 2
psw history delete 3

# 5. View final history
psw history list
```

### Recovery Scenario

```bash
# You generated a perfect script last week but forgot the exact config

# 1. List history
psw history list --history-limit 30

# 3. Re-run it
psw history rerun 15
# Regenerates with exact same settings

# 4. Execute in new location
# ... follows prompts to execute in new directory
```

## Integration with Templates

History and templates work great together:

```bash
# Create template
psw template save my-standard [params]

# Use local template (creates history entry)
psw template load my-standard -n Project1

# Later, re-run the exact same thing from history
psw history list
psw history rerun 1

# Or use the template again for a new project
psw template load my-standard -n Project2
```

Both approaches are tracked in history!

## Privacy & Security

### What's Stored

History stores:
- Script content
- Configuration (packages, project names, settings)
- Templates used
- Execution status
- Passwords are stored in plain text
- Connection strings are stored as configured (be careful!)

### What's NOT Stored

- API keys or sensitive environment variables

### Security Best Practices

1. **Don't commit history to Git**:
   ```bash
   # Add to .gitignore
   .psw/history/
   ```

2. **Review before sharing**:
   ```bash
   psw history show 1
   # Check for sensitive data before sharing
   ```

3. **Use environment variables** for sensitive data:
   ```bash
   # In templates/scripts, use:
   password: ${ADMIN_PASS}
   # Instead of literal passwords
   ```

4. **Regularly clear** old history with sensitive data:
   ```bash
   psw history clear
   ```

## See Also

- [TEMPLATES.md](TEMPLATES.md) - Template system documentation
- [README.md](README.md) - General CLI documentation
- [API Reference](../../.github/api-reference.md) - API endpoints
