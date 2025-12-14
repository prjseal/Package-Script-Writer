# Template Commands - Process Flow Diagrams

This document contains detailed process flow diagrams for all `psw template` commands using Mermaid flowcharts.

## Table of Contents
- [Template Save](#template-save)
- [Template Load](#template-load)
- [Template List](#template-list)
- [Template Show](#template-show)
- [Template Delete](#template-delete)
- [Template Export](#template-export)
- [Template Import](#template-import)
- [Template Validate](#template-validate)

---

## Template Save

**Command**: `psw template save <name> [options]`

**Purpose**: Saves a template from current configuration to disk.

**File Reference**: `src/PackageCliTool/Workflows/TemplateWorkflow.cs:109-162`

```mermaid
flowchart TD
    Start([Start: template save]) --> CheckName{Name<br/>provided?}
    CheckName -->|No| PromptName[Prompt for template name]
    CheckName -->|Yes| HasDesc{Description<br/>provided?}
    PromptName --> HasDesc

    HasDesc -->|No| PromptDesc[Prompt for description]
    HasDesc -->|Yes| CreateTemplate[Create template from options]
    PromptDesc --> CreateTemplate

    CreateTemplate --> HasTags{Tags<br/>provided?}
    HasTags -->|No| PromptTags[Prompt for tags]
    HasTags -->|Yes| AddTags[Add tags to template]
    PromptTags --> AddTags

    AddTags --> Validate[Validate template<br/>TemplateValidator.ValidateAndThrow]
    Validate --> Exists{Template<br/>exists?}

    Exists -->|No| Save[Save template to disk<br/>~/.psw/templates/]
    Exists -->|Yes| ConfirmOverwrite{Confirm<br/>overwrite?}

    ConfirmOverwrite -->|No| Cancel[Display: Template save cancelled]
    ConfirmOverwrite -->|Yes| Save

    Save --> Success[Display: ✓ Template saved]
    Success --> End([Exit])
    Cancel --> End

    style Start fill:#e1f5e1
    style End fill:#ffe1e1
    style Save fill:#e3f2fd
    style Validate fill:#fff3e0
    style Success fill:#c8e6c9
    style Cancel fill:#ffccbc
```

**Key States**:
1. `Initialization` → Input validation
2. `PromptForDetails` → Name, description, tags
3. `CreateTemplate` → Build template object
4. `ValidateTemplate` → Run validation rules
5. `CheckExists` → Check for conflicts
6. `SaveToDisk` → Write YAML file
7. `Exit`

---

## Template Load

**Command**: `psw template load <name> [options]`

**Purpose**: Loads and executes a saved template with optional overrides using standard CLI options.

**File Reference**: `src/PackageCliTool/Workflows/TemplateWorkflow.cs:167-271`

```mermaid
flowchart TD
    Start([Start: template load]) --> CheckName{Name<br/>provided?}
    CheckName -->|No| ListCheck{Templates<br/>exist?}
    CheckName -->|Yes| LoadTemplate[Load template from disk]

    ListCheck -->|No| NoTemplates[Display: No templates found]
    ListCheck -->|Yes| PromptSelect[Display selection prompt<br/>Choose from list]

    NoTemplates --> End([Exit])
    PromptSelect --> LoadTemplate

    LoadTemplate --> ShowLoaded[Display: ✓ Template loaded]
    ShowLoaded --> ApplyOverrides[Apply command-line overrides<br/>-n, -s, -p, -t, -k, etc. same as --default]

    ApplyOverrides --> ConvertModel[Convert to ScriptModel]
    ConvertModel --> NeedPassword{Unattended +<br/>No password?}

    NeedPassword -->|Yes| PromptPassword[Prompt for admin password<br/>Min 10 characters, secret]
    NeedPassword -->|No| DisplaySummary[Display configuration summary]
    PromptPassword --> DisplaySummary

    DisplaySummary --> GenerateScript[Generate script<br/>ScriptGeneratorService]
    GenerateScript --> SaveHistory[Save to history]
    SaveHistory --> DisplayScript[Display generated script]

    DisplayScript --> ScriptActions[Handle Script Actions]
    ScriptActions --> ActionPrompt{User<br/>action?}

    ActionPrompt -->|Run| PromptDir[Prompt for run directory]
    ActionPrompt -->|Edit| EditMsg[Display: Use interactive mode]
    ActionPrompt -->|Copy| CopyClip[Copy to clipboard]
    ActionPrompt -->|Save| SaveAsTemplate[Save as new template]
    ActionPrompt -->|Start over| StartOverMsg[Display: Re-run with different options]

    PromptDir --> ValidateDir{Directory<br/>exists?}
    ValidateDir -->|No| ConfirmCreate{Confirm<br/>create?}
    ValidateDir -->|Yes| RunScript[Execute script]

    ConfirmCreate -->|No| CancelRun[Display: Cancelled]
    ConfirmCreate -->|Yes| CreateDir[Create directory]
    CreateDir --> RunScript

    RunScript --> End
    CancelRun --> End
    EditMsg --> End
    StartOverMsg --> End

    CopyClip --> Continue1{Continue?}
    SaveAsTemplate --> Continue2{Continue?}

    Continue1 -->|Yes| ScriptActions
    Continue1 -->|No| End
    Continue2 -->|Yes| ScriptActions
    Continue2 -->|No| End

    style Start fill:#e1f5e1
    style End fill:#ffe1e1
    style LoadTemplate fill:#e3f2fd
    style GenerateScript fill:#fff3e0
    style RunScript fill:#c8e6c9
    style DisplayScript fill:#e1bee7
```

**Key States**:
1. `Initialization` → Check if name provided
2. `ListTemplates` → Show available templates (if no name)
3. `LoadFromDisk` → Read YAML file
4. `ApplyOverrides` → Merge command-line overrides
5. `GenerateScript` → Create installation script
6. `DisplayScript` → Show generated output
7. `ScriptActions` → Interactive action menu
8. `Exit`

---

## Template List

**Command**: `psw template list`

**Purpose**: Lists all available templates in table format.

**File Reference**: `src/PackageCliTool/Workflows/TemplateWorkflow.cs:424-463`

```mermaid
flowchart TD
    Start([Start: template list]) --> ListTemplates[List templates from<br/>~/.psw/templates/]
    ListTemplates --> CheckCount{Templates<br/>found?}

    CheckCount -->|No| NoTemplates[Display: No templates found<br/>Create one with 'psw template save']
    CheckCount -->|Yes| BuildTable[Build table with columns:<br/>Name, Description, Version,<br/>Tags, Modified]

    BuildTable --> DisplayTable[Display table with<br/>rounded border]
    DisplayTable --> ShowCount[Display: Total count]
    ShowCount --> ShowHelp[Display helpful commands:<br/>- psw template show name<br/>- psw template load name]

    ShowHelp --> End([Exit])
    NoTemplates --> End

    style Start fill:#e1f5e1
    style End fill:#ffe1e1
    style BuildTable fill:#e3f2fd
    style DisplayTable fill:#c8e6c9
```

**Key States**:
1. `Initialization`
2. `ListTemplates` → Read all .yaml files
3. `CheckEmpty` → Validate count
4. `DisplayTable` → Show formatted output
5. `Exit`

---

## Template Show

**Command**: `psw template show <name>`

**Purpose**: Displays detailed metadata and configuration of a specific template.

**File Reference**: `src/PackageCliTool/Workflows/TemplateWorkflow.cs:468-527`

```mermaid
flowchart TD
    Start([Start: template show]) --> CheckName{Name<br/>provided?}
    CheckName -->|No| PromptName[Prompt for template name]
    CheckName -->|Yes| LoadTemplate[Load template from disk]
    PromptName --> LoadTemplate

    LoadTemplate --> BuildMetadata[Build metadata table:<br/>Description, Author, Version,<br/>Created, Modified, Tags]

    BuildMetadata --> DisplayMetadata[Display metadata table<br/>Blue rounded border]
    DisplayMetadata --> BuildConfig[Build configuration table:<br/>Template, Project, Solution,<br/>Packages, Starter Kit,<br/>Docker, Unattended]

    BuildConfig --> ShowPackages{Packages<br/>exist?}
    ShowPackages -->|Yes| AddPackages[Add package details:<br/>└─ PackageName @ Version]
    ShowPackages -->|No| DisplayConfig[Display configuration table<br/>Green rounded border]
    AddPackages --> DisplayConfig

    DisplayConfig --> End([Exit])

    style Start fill:#e1f5e1
    style End fill:#ffe1e1
    style LoadTemplate fill:#e3f2fd
    style DisplayMetadata fill:#bbdefb
    style DisplayConfig fill:#c8e6c9
```

**Key States**:
1. `Initialization` → Check if name provided
2. `LoadTemplate` → Read template from disk
3. `DisplayMetadata` → Show template information
4. `DisplayConfiguration` → Show settings
5. `Exit`

---

## Template Delete

**Command**: `psw template delete <name>`

**Purpose**: Deletes a template after confirmation.

**File Reference**: `src/PackageCliTool/Workflows/TemplateWorkflow.cs:532-553`

```mermaid
flowchart TD
    Start([Start: template delete]) --> CheckName{Name<br/>provided?}
    CheckName -->|No| PromptName[Prompt for template name]
    CheckName -->|Yes| Confirm{Confirm<br/>deletion?}
    PromptName --> Confirm

    Confirm -->|No| Cancel[Display: Delete cancelled]
    Confirm -->|Yes| CheckExists{Template<br/>exists?}

    CheckExists -->|No| Error[Error: Template not found<br/>Show available templates]
    CheckExists -->|Yes| DeleteFile[Delete .yaml file from<br/>~/.psw/templates/]

    DeleteFile --> Success[Display: ✓ Template deleted]
    Success --> End([Exit])
    Cancel --> End
    Error --> End

    style Start fill:#e1f5e1
    style End fill:#ffe1e1
    style DeleteFile fill:#ffccbc
    style Success fill:#c8e6c9
    style Cancel fill:#fff9c4
    style Error fill:#ffccbc
```

**Key States**:
1. `Initialization` → Check if name provided
2. `PromptConfirmation` → Ask user to confirm
3. `CheckExists` → Validate template exists
4. `DeleteFromDisk` → Remove file
5. `Exit`

---

## Template Export

**Command**: `psw template export <name> [--file <path>]`

**Purpose**: Exports a template to a specific file path.

**File Reference**: `src/PackageCliTool/Workflows/TemplateWorkflow.cs:558-574`

```mermaid
flowchart TD
    Start([Start: template export]) --> CheckName{Name<br/>provided?}
    CheckName -->|No| PromptName[Prompt for template name]
    CheckName -->|Yes| CheckPath{Output path<br/>provided?}
    PromptName --> CheckPath

    CheckPath -->|No| DefaultPath[Use default:<br/>name.yaml]
    CheckPath -->|Yes| LoadTemplate[Load template from<br/>~/.psw/templates/]
    DefaultPath --> LoadTemplate

    LoadTemplate --> Serialize[Serialize template to YAML]
    Serialize --> WriteFile[Write to output file path]
    WriteFile --> Success[Display: ✓ Template exported to: path]

    Success --> End([Exit])

    style Start fill:#e1f5e1
    style End fill:#ffe1e1
    style LoadTemplate fill:#e3f2fd
    style Serialize fill:#fff3e0
    style WriteFile fill:#e3f2fd
    style Success fill:#c8e6c9
```

**Key States**:
1. `Initialization` → Validate inputs
2. `LoadTemplate` → Read from templates directory
3. `SerializeYAML` → Convert to YAML format
4. `WriteToFile` → Save to specified path
5. `Exit`

---

## Template Import

**Command**: `psw template import <filepath> [--name <newname>]`

**Purpose**: Imports a template from a file into the templates directory.

**File Reference**: `src/PackageCliTool/Workflows/TemplateWorkflow.cs:579-594`

```mermaid
flowchart TD
    Start([Start: template import]) --> CheckFile{File path<br/>provided?}
    CheckFile -->|No| PromptFile[Prompt for template file path]
    CheckFile -->|Yes| FileExists{File<br/>exists?}
    PromptFile --> FileExists

    FileExists -->|No| Error[Error: Template file not found]
    FileExists -->|Yes| ReadFile[Read YAML file]

    ReadFile --> Deserialize[Deserialize to Template object]
    Deserialize --> CheckRename{New name<br/>provided?}

    CheckRename -->|Yes| RenameTemplate[Update template name]
    CheckRename -->|No| SaveTemplate[Save to ~/.psw/templates/]
    RenameTemplate --> SaveTemplate

    SaveTemplate --> Success[Display: ✓ Template imported: name]
    Success --> End([Exit])
    Error --> End

    style Start fill:#e1f5e1
    style End fill:#ffe1e1
    style ReadFile fill:#e3f2fd
    style Deserialize fill:#fff3e0
    style SaveTemplate fill:#e3f2fd
    style Success fill:#c8e6c9
    style Error fill:#ffccbc
```

**Key States**:
1. `Initialization` → Check file path
2. `ValidateFile` → Check file exists
3. `ReadYAML` → Load file contents
4. `Deserialize` → Parse YAML to object
5. `RenameOptional` → Apply new name if provided
6. `SaveToTemplates` → Write to templates directory
7. `Exit`

---

## Template Validate

**Command**: `psw template validate <filepath>`

**Purpose**: Validates a template file without importing it.

**File Reference**: `src/PackageCliTool/Workflows/TemplateWorkflow.cs:599-635`

```mermaid
flowchart TD
    Start([Start: template validate]) --> CheckFile{File path<br/>provided?}
    CheckFile -->|No| PromptFile[Prompt for template file path]
    CheckFile -->|Yes| FileExists{File<br/>exists?}
    PromptFile --> FileExists

    FileExists -->|No| Error[Error: Template file not found]
    FileExists -->|Yes| ReadFile[Read YAML file]

    ReadFile --> TryImport[Import as temp template<br/>temp-guid]
    TryImport --> ImportSuccess{Import<br/>successful?}

    ImportSuccess -->|No| ValidationError[Display: ✗ Validation failed<br/>Show error message]
    ImportSuccess -->|Yes| RunValidation[Run TemplateValidator.Validate]

    RunValidation --> DeleteTemp[Delete temporary template]
    DeleteTemp --> CheckErrors{Validation<br/>errors?}

    CheckErrors -->|No| Success[Display: ✓ Template is valid]
    CheckErrors -->|Yes| Warnings[Display: ⚠ Warnings<br/>List all validation errors]

    Success --> End([Exit])
    Warnings --> End
    ValidationError --> End
    Error --> End

    style Start fill:#e1f5e1
    style End fill:#ffe1e1
    style ReadFile fill:#e3f2fd
    style RunValidation fill:#fff3e0
    style Success fill:#c8e6c9
    style Warnings fill:#fff9c4
    style ValidationError fill:#ffccbc
    style Error fill:#ffccbc
```

**Key States**:
1. `Initialization` → Check file path
2. `ValidateFile` → Check file exists
3. `TryImport` → Attempt to import as temp
4. `RunValidation` → Execute validation rules
5. `CleanupTemp` → Remove temporary template
6. `DisplayResults` → Show validation outcome
7. `Exit`

---

## Common Workflow Elements

### Script Actions (Post-Generation)

After generating a script from `template load`, users can choose from these actions:

```mermaid
flowchart TD
    Actions{Choose<br/>Action}

    Actions -->|Run| PromptDir[Prompt for run directory]
    Actions -->|Edit| EditMsg[Use interactive mode message]
    Actions -->|Copy| CopyClip[Copy script to clipboard]
    Actions -->|Save| SaveTemplate[Save as new template]
    Actions -->|Start over| StartOver[Re-run with different options]

    PromptDir --> ValidateDir{Dir<br/>exists?}
    ValidateDir -->|No| CreateDir[Create directory]
    ValidateDir -->|Yes| Execute[Execute script]
    CreateDir --> Execute

    CopyClip --> Continue1{Continue?}
    SaveTemplate --> Continue2{Continue?}

    Continue1 -->|Yes| Actions
    Continue1 -->|No| Exit([Exit])
    Continue2 -->|Yes| Actions
    Continue2 -->|No| Exit

    Execute --> Exit
    EditMsg --> Exit
    StartOver --> Exit

    style Actions fill:#fff3e0
    style Execute fill:#c8e6c9
    style CopyClip fill:#bbdefb
    style SaveTemplate fill:#e1bee7
```

### Error Handling

All template commands include error handling:

```mermaid
flowchart TD
    Operation[Any Template Operation] --> Try{Try<br/>Execute}
    Try -->|Success| Success[Display success message]
    Try -->|Exception| Catch[Catch Exception]

    Catch --> LogError[Log error details]
    LogError --> DisplayError[Display error message<br/>with suggestion]

    DisplayError --> SetExitCode[Set Environment.ExitCode = 1]
    SetExitCode --> Exit([Exit])
    Success --> Exit

    style Operation fill:#e3f2fd
    style Success fill:#c8e6c9
    style Catch fill:#ffccbc
    style DisplayError fill:#ffccbc
```

---

## State Transition Summary

### Template Command Router

```mermaid
stateDiagram-v2
    [*] --> Initialization
    Initialization --> ValidateCommand

    ValidateCommand --> SaveTemplate: command = "save"
    ValidateCommand --> LoadTemplate: command = "load"
    ValidateCommand --> ListTemplates: command = "list"
    ValidateCommand --> ShowTemplate: command = "show"
    ValidateCommand --> DeleteTemplate: command = "delete"
    ValidateCommand --> ExportTemplate: command = "export"
    ValidateCommand --> ImportTemplate: command = "import"
    ValidateTemplate: command = "validate"
    ValidateCommand --> ShowHelp: unknown/empty

    SaveTemplate --> Exit
    LoadTemplate --> ScriptActions
    ScriptActions --> Exit
    ListTemplates --> Exit
    ShowTemplate --> Exit
    DeleteTemplate --> Exit
    ExportTemplate --> Exit
    ImportTemplate --> Exit
    ValidateTemplate --> Exit
    ShowHelp --> Exit

    Exit --> [*]
```

---

## File Locations

**Templates Storage**: `~/.psw/templates/*.yaml`

**Source Files**:
- `src/PackageCliTool/Workflows/TemplateWorkflow.cs` - Main workflow orchestration
- `src/PackageCliTool/Services/TemplateService.cs` - CRUD operations
- `src/PackageCliTool/Models/Templates/Template.cs` - Template model
- `src/PackageCliTool/Validation/TemplateValidator.cs` - Validation rules

---

## Notes

1. All template operations use YAML serialization/deserialization
2. Templates are stored as individual `.yaml` files in the user's home directory
3. Template validation runs automatically on save and import
4. The `load` command supports runtime overrides using standard CLI options (same as `--default` mode)
5. Password fields support three formats: literal, `${ENV_VAR}`, or `<prompt>`
6. Templates include metadata (name, description, author, version, tags, timestamps)
7. Script generation from templates uses the same engine as interactive mode
