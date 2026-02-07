# CLI Agent Improvements: Making PSW CLI AI-Friendly

Analysis of the Package Script Writer CLI tool, focused on how AI agents (Claude, Copilot, ChatGPT, custom agents) can understand and use this tool effectively. This covers documentation, help text, output format, and discoverability.

---

## Current State Assessment

The CLI is well-built for human interactive use. Spectre.Console provides a polished terminal experience. However, several patterns make it harder for AI agents to use effectively:

1. **Help text is rendered with Spectre.Console markup** (`[green]`, `[cyan]`, etc.), which is great visually but produces noisy output when an agent reads it. AI agents parse raw text; ANSI escape codes and markup tags are clutter.
2. **No machine-readable output mode.** Every output path uses Spectre.Console formatting. There is no `--output json` or `--output plain` mode for agents to consume structured results.
3. **Exit codes are limited.** The tool exits with `0` (success) or `1` (any error). There is no differentiation between validation errors, network errors, and execution errors at the process level.
4. **The CLI mode workflow still prompts interactively** in some paths (e.g., `HandleInteractiveScriptActionAsync` after script generation when `--auto-run` is not set), which blocks AI agents that cannot respond to stdin prompts.
5. **Flag semantics have subtle ambiguities.** For example, `-t` treats a bare value as a version, but `--template-package` treats a bare value as a package name. This is documented in the help text but requires careful reading.
6. **No command to list valid values.** An agent cannot ask the CLI "what database types are valid?" or "what starter kits are available?" without reading source code.

---

## Prioritised Improvements

### Priority 1: Critical for AI Agent Usability

#### 1.1 Add `--output` flag for machine-readable output

**Problem:** All output uses Spectre.Console markup. An AI agent running `psw --default` gets a script wrapped in a decorative panel with ANSI escape codes. Parsing the actual script content from that output is fragile.

**Recommendation:** Add `--output <format>` flag supporting:
- `--output json` - Returns structured JSON with the generated script, configuration used, and metadata
- `--output plain` - Returns the raw script text only, no panels, no colours, no spinners
- Default (no flag) - Current behaviour, unchanged

**Example JSON output:**
```json
{
  "success": true,
  "script": "#!/bin/bash\n# Install Umbraco...",
  "configuration": {
    "templateName": "Umbraco.Templates",
    "templateVersion": "17.0.3",
    "projectName": "MyProject",
    "packages": ["uSync|17.0.0"],
    "databaseType": "SQLite"
  },
  "metadata": {
    "generatedAt": "2025-01-15T10:30:00Z",
    "cliVersion": "1.1.2",
    "historyId": 42
  }
}
```

**Impact:** This is the single most important improvement. Without it, every AI agent has to parse ANSI-decorated text to extract useful content.

**Files affected:**
- `Models/CommandLineOptions.cs` - Add `OutputFormat` property and parsing
- `UI/ConsoleDisplay.cs` - Add plain/JSON rendering paths
- `Workflows/CliModeWorkflow.cs` - Conditionally suppress Spectre output
- `Program.cs` - Route output format through workflows

---

#### 1.2 Add `--no-interaction` / `--non-interactive` flag

**Problem:** Even in CLI mode, some code paths call `InteractivePrompts.PromptForScriptAction()` which blocks waiting for user input. An AI agent cannot respond to Spectre.Console prompts on stdin. When `--auto-run` is not set and the script is generated, the workflow still prompts "What would you like to do with this script?" (`CliModeWorkflow.cs:439`).

**Recommendation:** Add a `--no-interaction` flag that:
- Suppresses all interactive prompts
- Falls back to sensible defaults (print the script, exit)
- Returns non-zero exit code if a prompt would have been required but cannot proceed

**Files affected:**
- `Models/CommandLineOptions.cs` - Add `NonInteractive` property
- `Workflows/CliModeWorkflow.cs` - Skip prompts when non-interactive
- `UI/InteractivePrompts.cs` - Guard all prompts

---

#### 1.3 Add `--script-only` flag to output just the script

**Problem:** An AI agent that wants to capture the generated script has to strip the Spectre.Console panel, figlet banner, status spinners, and colour codes. This is the most common agent use case: "generate a script and give it to me."

**Recommendation:** Add `--script-only` flag that outputs only the raw script text to stdout with no decoration. This is simpler than `--output plain` and covers the most common case.

**Implementation:** When set, skip `ConsoleDisplay.DisplayGeneratedScript()` and instead write the raw script string to `Console.Out`.

**Files affected:**
- `Models/CommandLineOptions.cs` - Add `ScriptOnly` property
- `Workflows/CliModeWorkflow.cs` - Conditional output path
- `Program.cs` - Suppress banner/spinners when script-only

---

### Priority 2: Important for Documentation and Discoverability

#### 2.1 Add `--help-json` flag for machine-readable help

**Problem:** The current `--help` output is a Spectre.Console panel with markup tags. An AI agent reading this gets text like `[green]    --admin-email[/] <email>     Admin email for unattended install`. The agent has to strip markup to understand the options.

**Recommendation:** Add `--help-json` that outputs a structured JSON schema of all commands, flags, their types, defaults, valid values, and descriptions.

**Example output:**
```json
{
  "name": "psw",
  "description": "Package Script Writer - Generate Umbraco CMS installation scripts",
  "version": "1.1.2",
  "commands": [
    {
      "name": "template",
      "description": "Manage script configuration templates",
      "subcommands": [
        {
          "name": "save",
          "description": "Save current configuration as a template",
          "arguments": [{"name": "name", "required": true, "description": "Template name"}]
        }
      ]
    }
  ],
  "options": [
    {
      "name": "--packages",
      "shortName": "-p",
      "type": "string",
      "required": false,
      "description": "Comma-separated list of packages with optional versions",
      "format": "Package1|Version1,Package2|Version2",
      "examples": ["uSync|17.0.0,Umbraco.Forms", "uSync,Diplo.GodMode"]
    },
    {
      "name": "--database-type",
      "type": "enum",
      "required": false,
      "validValues": ["SQLite", "LocalDb", "SQLServer", "SQLAzure", "SQLCE"],
      "default": null,
      "description": "Database type for unattended install"
    }
  ]
}
```

**Impact:** Allows AI agents to programmatically discover capabilities without parsing decorated text.

**Files affected:**
- `Models/CommandLineOptions.cs` - Add `ShowHelpJson` property
- `UI/ConsoleDisplay.cs` - Add `DisplayHelpJson()` method
- `Program.cs` - Handle the flag

---

#### 2.2 Add `psw list-options` subcommand for valid value discovery

**Problem:** An AI agent does not know what database types, starter kits, or template packages are valid without reading source code. The valid database types are hardcoded in `InputValidator.cs:213` as `["SQLite", "LocalDb", "SQLServer", "SQLAzure", "SQLCE"]`. The starter kits are hardcoded in `InteractivePrompts.cs:76-85`.

**Recommendation:** Add a `list-options` subcommand:
```bash
psw list-options                    # List all option categories
psw list-options database-types     # List valid database types
psw list-options starter-kits       # List available starter kits
psw list-options template-packages  # List known template packages
```

**With `--output json`:**
```json
{
  "databaseTypes": ["SQLite", "LocalDb", "SQLServer", "SQLAzure", "SQLCE"],
  "starterKits": ["clean", "Articulate", "Portfolio", "LittleNorth.Igloo", "Umbraco.BlockGrid.Example.Website", "Umbraco.TheStarterKit", "uSkinnedSiteBuilder"],
  "defaultValues": {
    "projectName": "MyProject",
    "solutionName": "MySolution",
    "databaseType": "SQLite",
    "adminEmail": "admin@example.com"
  }
}
```

**Impact:** Agents can dynamically discover valid inputs instead of guessing or relying on stale documentation.

**Files affected:**
- New workflow: `Workflows/ListOptionsWorkflow.cs`
- `Models/CommandLineOptions.cs` - Add parsing for `list-options`
- `Program.cs` - Route to new workflow

---

#### 2.3 Improve `--help` text with explicit type annotations and defaults

**Problem:** The current help text does not consistently show:
- What type each option expects (string, boolean, enum)
- What the default value is
- Whether the option is required or optional
- What other options it depends on (e.g., `--connection-string` is required when `--database-type` is `SQLServer`)

**Current:**
```
--database-type <type>    Database type (SQLite, LocalDb, SQLServer, SQLAzure, SQLCE)
```

**Recommended:**
```
--database-type <type>    Database type [enum: SQLite, LocalDb, SQLServer, SQLAzure, SQLCE]
                          Default: none. Requires: --unattended-defaults or related flags.
                          Note: SQLServer and SQLAzure require --connection-string.
```

**Impact:** AI agents and humans both benefit from explicit type/default/dependency documentation directly in help text.

**Files affected:**
- `UI/ConsoleDisplay.cs` - Expand help text descriptions

---

#### 2.4 Add a `--dry-run` flag

**Problem:** An AI agent may want to validate a command without actually generating a script or hitting APIs. Currently there is no way to check "would this command succeed?" without running it.

**Recommendation:** Add `--dry-run` that:
- Validates all inputs
- Reports what would be generated (configuration summary)
- Does not call any APIs or generate scripts
- Returns exit code 0 if inputs are valid, non-zero if not

**Example:**
```bash
psw --dry-run -p "uSync|17.0.0" -n MyProject --database-type InvalidDB
# Exit code 1
# Error: Invalid database type: InvalidDB. Valid values: SQLite, LocalDb, SQLServer, SQLAzure, SQLCE
```

**Files affected:**
- `Models/CommandLineOptions.cs` - Add `DryRun` property
- `Workflows/CliModeWorkflow.cs` - Short-circuit after validation

---

### Priority 3: Valuable Enhancements

#### 3.1 Use distinct exit codes for different error categories

**Problem:** The tool always exits with code `1` for any error (`Program.cs:245`). An AI agent cannot distinguish between "invalid input" (fixable by changing arguments), "network error" (retryable), and "script execution failed" (investigate output).

**Recommendation:** Use distinct exit codes:
| Code | Meaning |
|------|---------|
| 0 | Success |
| 1 | General/unknown error |
| 2 | Invalid arguments / validation error |
| 3 | Network / API error |
| 4 | Script execution failed |
| 5 | File system / permission error |

**Files affected:**
- `Program.cs` - Map exception types to exit codes
- `Exceptions/PswException.cs` - Add `ExitCode` property to base exception

---

#### 3.2 Add `--version --output json` for structured version info

**Problem:** `psw --version` outputs a Figlet banner plus text. An agent checking whether the tool is installed or what version is running has to parse decorative text.

**Recommendation:** When `--output json` is combined with `--version`:
```json
{
  "name": "PackageScriptWriter.Cli",
  "version": "1.1.2",
  "runtime": ".NET 10.0",
  "platform": "linux-x64"
}
```

When `--output plain` is combined with `--version`:
```
1.1.2
```

**Files affected:**
- `UI/ConsoleDisplay.cs` - Add plain/JSON version output

---

#### 3.3 Add stderr/stdout separation

**Problem:** Currently, status messages ("Generating script..."), errors, and the actual script output all go to stdout via `AnsiConsole`. An AI agent piping stdout to capture the script also captures spinners and status messages.

**Recommendation:** Follow Unix conventions:
- **stdout**: The generated script (the useful output)
- **stderr**: Status messages, spinners, errors, warnings

This enables `psw --default 2>/dev/null` to get just the script.

**Impact:** Works with standard Unix tooling and piping, which AI agents commonly use.

**Files affected:**
- `Workflows/CliModeWorkflow.cs` - Route status to stderr
- `UI/ConsoleDisplay.cs` - Use `Console.Error` for non-data output
- `Services/ScriptExecutor.cs` - Route execution status to stderr

---

#### 3.4 Document the CLI for AI agents explicitly

**Problem:** The existing README is written for human developers. AI agents benefit from a different documentation format: a concise reference card with exact command syntax, all valid values, and copy-paste examples for common workflows.

**Recommendation:** Add an `AI-USAGE.md` or `AGENT-GUIDE.md` file (or a section in the README) with:

1. **Quick reference table** of all flags with types, defaults, and valid values
2. **Common AI agent workflows** as exact copy-paste commands:
   - "Generate a default Umbraco project script"
   - "Generate a script with specific packages"
   - "Generate a script with full unattended install configuration"
   - "List available community templates"
   - "Save and reuse a configuration template"
3. **Input format specifications** - Exact syntax for the pipe-separated package format, version format, etc.
4. **Error code reference** - What each error code means and how to handle it
5. **Exit code reference** - What each exit code means

**Files affected:**
- New file: `src/PackageCliTool/AI-USAGE.md` or section in README

---

#### 3.5 Resolve the `-t` vs `--template-package` ambiguity

**Problem:** The `-t` and `--template-package` flags behave differently for bare values (no pipe):
- `-t 17.0.3` sets `TemplateVersion = "17.0.3"` (treats bare value as version)
- `--template-package Umbraco.Templates` sets `TemplatePackageName = "Umbraco.Templates"` (treats bare value as package name)

This is documented but unintuitive. An AI agent that learns one flag's behaviour may incorrectly assume the other works the same way.

**Recommendation:** Either:
- Make them consistent (both treat bare values the same way), or
- Deprecate the ambiguous shorthand and introduce explicit `--template-version` flag, or
- At minimum, add a prominent note in help text explaining the difference

**Files affected:**
- `Models/CommandLineOptions.cs` - Align parsing logic or add `--template-version`
- `UI/ConsoleDisplay.cs` - Update help text

---

### Priority 4: Nice-to-Have

#### 4.1 Add `--validate` flag for input checking

**Problem:** An AI agent building up a command incrementally cannot check if partial input is valid. The validation currently happens deep inside the workflow.

**Recommendation:** `psw --validate --database-type SQLite --admin-email bad` would validate inputs and report all validation errors without generating anything.

Difference from `--dry-run`: `--validate` only checks input format, `--dry-run` would also check API reachability and resolve package names.

---

#### 4.2 Add shell completion scripts

**Problem:** AI agents using shell often benefit from tab completion data to understand available commands. Generating shell completions also serves as structured documentation.

**Recommendation:** Add `psw completion bash|zsh|fish|powershell` that outputs shell completion scripts. This is standard practice for modern CLI tools and provides a machine-parseable description of all commands and options.

---

#### 4.3 Support reading configuration from stdin or file

**Problem:** Complex configurations require very long command lines. An AI agent generating a script with many packages, custom credentials, Docker options, and template settings produces an unwieldy single command.

**Recommendation:** Support `psw --config config.json` or `cat config.json | psw --config -` where `config.json` is a JSON file matching the `ScriptModel` schema.

**Example:**
```json
{
  "templateName": "Umbraco.Templates",
  "templateVersion": "17.0.3",
  "projectName": "MyBlog",
  "packages": "uSync|17.0.0,Diplo.GodMode",
  "databaseType": "SQLite",
  "useUnattendedInstall": true,
  "adminEmail": "admin@example.com",
  "adminPassword": "1234567890"
}
```

**Impact:** Enables structured input rather than complex flag parsing, which is the natural output format for AI agents.

---

#### 4.4 Add `psw explain` subcommand

**Problem:** An AI agent may want to understand what a specific generated script does before recommending execution.

**Recommendation:** `psw explain --default` would output a human/AI-readable explanation of what the default script does, step by step, without generating the actual script. This helps agents provide context to users.

---

## Summary: Implementation Order

| # | Improvement | Priority | Effort | AI Impact |
|---|-------------|----------|--------|-----------|
| 1.1 | `--output json/plain` flag | Critical | Medium | Very High |
| 1.2 | `--no-interaction` flag | Critical | Low | Very High |
| 1.3 | `--script-only` flag | Critical | Low | High |
| 2.1 | `--help-json` structured help | Important | Medium | High |
| 2.2 | `psw list-options` subcommand | Important | Medium | High |
| 2.3 | Improve help text with types/defaults | Important | Low | Medium |
| 2.4 | `--dry-run` flag | Important | Low | Medium |
| 3.1 | Distinct exit codes | Valuable | Low | Medium |
| 3.2 | Structured version output | Valuable | Low | Low |
| 3.3 | stderr/stdout separation | Valuable | Medium | High |
| 3.4 | AI agent documentation | Valuable | Low | High |
| 3.5 | Resolve `-t` ambiguity | Valuable | Low | Medium |
| 4.1 | `--validate` flag | Nice-to-have | Low | Low |
| 4.2 | Shell completion scripts | Nice-to-have | Medium | Low |
| 4.3 | Config from stdin/file | Nice-to-have | Medium | High |
| 4.4 | `psw explain` subcommand | Nice-to-have | Medium | Medium |

---

## What Already Works Well

Credit where due -- these aspects of the current CLI are already AI-friendly:

- **Comprehensive CLI mode**: The tool already supports full non-interactive operation via flags. An agent can construct a complete command without interactive prompts (as long as `--auto-run` is set or the script output is the goal).
- **Good error messages**: The `PswException` hierarchy with `UserMessage`, `Suggestion`, and `ErrorCode` is excellent. Agents can parse these structured error messages.
- **Input validation**: The `InputValidator` class provides clear, specific validation errors with field names and suggestions. This helps agents understand what went wrong.
- **Template system**: YAML-based templates with save/load/export/import provide a workflow that agents can use to persist and share configurations.
- **History system**: The ability to `history list` and `history rerun` means agents can reference previous successful configurations.
- **Verbose logging**: `--verbose` provides detailed diagnostic output that helps agents troubleshoot failures.
