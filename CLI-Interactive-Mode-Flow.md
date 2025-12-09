# Interactive CLI Mode - Process Flow

This document describes the complete flow of the interactive CLI mode for the Package Script Writer tool, broken down into separate process diagrams for clarity.

---

## 1. Main Entry Flow

This diagram shows how the CLI starts and determines which workflow to execute.

```mermaid
flowchart TD
    Start([Start PSW CLI]) --> ParseArgs[Parse Command Line Arguments]
    ParseArgs --> InitLogging[Initialize Logging & Configuration]
    InitLogging --> InitServices[Initialize Services:<br/>- ApiClient<br/>- PackageSelector<br/>- ScriptExecutor<br/>- TemplateService<br/>- HistoryService]

    InitServices --> CheckFlags{Check Command<br/>Type}

    CheckFlags -->|--help| ShowHelp[Display Help]
    CheckFlags -->|--version| ShowVersion[Display Version]
    CheckFlags -->|--clear-cache| ClearCache[Clear Cache]
    CheckFlags -->|--update-packages| UpdateCache[Update Package Cache]
    CheckFlags -->|history command| HistoryWorkflow[Run History Workflow]
    CheckFlags -->|template command| TemplateWorkflow[Run Template Workflow]
    CheckFlags -->|Has CLI options| CliWorkflow[Run CLI Mode Workflow]
    CheckFlags -->|No options| InteractiveMode[Enter Interactive Mode]

    ShowHelp --> End([Exit])
    ShowVersion --> End
    ClearCache --> CheckMore{More to do?}
    UpdateCache --> CheckMore
    CheckMore -->|Yes| CheckFlags
    CheckMore -->|No| End

    HistoryWorkflow --> Success([Complete Successfully])
    TemplateWorkflow --> Success
    CliWorkflow --> Success
    InteractiveMode --> Success

    style Start fill:#90EE90
    style End fill:#FFB6C1
    style Success fill:#90EE90
    style InteractiveMode fill:#87CEEB
```

---

## 2. Interactive Mode - Main Flow

This diagram shows the high-level flow of interactive mode with the two main paths: Default and Custom.

```mermaid
flowchart TD
    Start([Enter Interactive Mode]) --> CtrlCWrapper[Wrap in Ctrl+C Handler]

    CtrlCWrapper -->|User presses Ctrl+C| Restart[Clear Screen<br/>Show Restart Message<br/>Wait 500ms]
    Restart --> CtrlCWrapper

    CtrlCWrapper -->|Normal Flow| DisplayBanner[Display Welcome Banner]
    DisplayBanner --> CheckUpdates[Check for Tool Updates<br/>Show if available]
    CheckUpdates --> PopulatePackages[Populate All Packages from API<br/>Show count loaded]

    PopulatePackages --> AskDefault{Ask: Generate<br/>Default Script?}

    AskDefault -->|Yes| DefaultPath[See: Default Script Flow]
    AskDefault -->|No| CustomPath[See: Custom Configuration Flow]

    DefaultPath --> ScriptActions[See: Script Actions Flow]
    CustomPath --> ScriptActions

    ScriptActions --> End([Exit Interactive Mode])

    style Start fill:#90EE90
    style End fill:#FFB6C1
    style DefaultPath fill:#FFE4B5
    style CustomPath fill:#FFE4B5
    style ScriptActions fill:#DDA0DD
```

---

## 3. Default Script Flow

This diagram shows the quick path for generating a default script with minimal configuration.

```mermaid
flowchart TD
    Start([Default Script Path]) --> ShowMessage[Display:<br/>'Generating Default Script'<br/>'Using default configuration']

    ShowMessage --> CreateModel[Create Default ScriptModel]

    CreateModel --> ModelDetails[Set Default Values:<br/>- Template: Umbraco.Templates latest<br/>- ProjectName: MyProject<br/>- CreateSolutionFile: true<br/>- SolutionName: MySolution<br/>- IncludeStarterKit: true<br/>- StarterKitPackage: clean<br/>- UseUnattendedInstall: true<br/>- DatabaseType: SQLite<br/>- UserEmail: admin@example.com<br/>- UserPassword: 1234567890<br/>- UserFriendlyName: Administrator<br/>- OnelinerOutput: false<br/>- RemoveComments: false]

    ModelDetails --> GenerateScript[Generate Script via API<br/>Show spinner animation]
    GenerateScript --> DisplayScript[Display Generated Script]
    DisplayScript --> NextStep[Continue to Script Actions]

    style Start fill:#FFE4B5
    style NextStep fill:#DDA0DD
```

---

## 4. Custom Flow - Steps 1 & 2 (Template Selection)

This diagram shows template and template version selection.

```mermaid
flowchart TD
    Start([Custom Flow Path]) --> Step1[Step 1: Select Template]

    Step1 --> ShowTemplates[Display Template Choices:<br/>- Umbraco.Templates<br/>- Umbraco.Community.Templates.Clean<br/>- Umbraco.Community.Templates.UmBootstrap]

    ShowTemplates --> SelectTemplate[User Selects Template]
    SelectTemplate --> ConfirmTemplate[Show confirmation message]

    ConfirmTemplate --> Step2[Step 2: Select Template Version]
    Step2 --> FetchVersions[Fetch Template Versions from NuGet<br/>Show spinner]

    FetchVersions --> ShowVersions[Display Version Choices:<br/>- Latest Stable<br/>- Pre-release<br/>- Specific Versions list]

    ShowVersions --> SelectVersion[User Selects Version]

    SelectVersion --> MapVersion{Which option?}
    MapVersion -->|Latest Stable| SetEmpty[Set version = empty string]
    MapVersion -->|Pre-release| SetPrerelease[Set version = '--prerelease']
    MapVersion -->|Specific Version| SetSpecific[Set version = selected version]

    SetEmpty --> ConfirmVersion[Show confirmation message]
    SetPrerelease --> ConfirmVersion
    SetSpecific --> ConfirmVersion

    ConfirmVersion --> NextStep[Continue to Step 3:<br/>Package Selection]

    style Start fill:#FFE4B5
    style Step1 fill:#F0E68C
    style Step2 fill:#F0E68C
    style NextStep fill:#F0E68C
```

---

## 5. Custom Flow - Step 3 (Package Selection)

This diagram shows the three different modes for selecting packages.

```mermaid
flowchart TD
    Start([Step 3: Select Packages]) --> AskMode{How to add packages?}

    AskMode -->|Select from popular| PopularMode[Popular Packages Mode]
    AskMode -->|Search for package| SearchMode[Search Mode]
    AskMode -->|None - skip| SkipMode[Skip Packages]

    %% Popular Mode
    PopularMode --> ShowAll[Display All Packages from API<br/>Multi-select prompt<br/>Page size: 10]
    ShowAll --> UserSelects[User selects packages<br/>Space to toggle<br/>Enter to confirm]
    UserSelects --> PackagesSelected[Packages Selected]

    %% Search Mode
    SearchMode --> SearchLoop[Search Loop Start]
    SearchLoop --> EnterTerm[Prompt: Enter search term]
    EnterTerm --> ValidateTerm{Term not empty?}
    ValidateTerm -->|No| EnterTerm
    ValidateTerm -->|Yes| SearchAPI[Search in:<br/>- Title<br/>- PackageId<br/>- Authors<br/>case-insensitive]

    SearchAPI --> FoundMatches{Found matches?}

    FoundMatches -->|Yes| DisplayMatches[Display matching packages<br/>Show: PackageId - Title by Authors<br/>Page size: 10]
    DisplayMatches --> UserPicksOne[User selects one package]
    UserPicksOne --> AddToList[Add to selected list<br/>Show confirmation]
    AddToList --> AskMore1{Search for another?}

    FoundMatches -->|No| ShowNoMatch[Show: No matches found]
    ShowNoMatch --> ValidNuGet{Valid NuGet<br/>Package ID format?}
    ValidNuGet -->|Yes| AskAddAnyway{Add anyway?}
    AskAddAnyway -->|Yes| AddToList
    AskAddAnyway -->|No| AskMore2{Search again?}
    ValidNuGet -->|No| ShowInvalid[Show: Not valid NuGet ID]
    ShowInvalid --> AskMore2

    AskMore1 -->|Yes| SearchLoop
    AskMore1 -->|No| PackagesSelected
    AskMore2 -->|Yes| SearchLoop
    AskMore2 -->|No| CheckIfAnyAdded{Any packages added?}
    CheckIfAnyAdded -->|Yes| PackagesSelected
    CheckIfAnyAdded -->|No| NoPackages

    %% Skip Mode
    SkipMode --> NoPackages[No Packages Selected]

    %% Results
    PackagesSelected --> CheckCount{Package count > 0?}
    CheckCount -->|Yes| NextStep[Continue to Step 4:<br/>Version Selection]
    CheckCount -->|No| ShowWarning[Show warning:<br/>No packages selected]

    NoPackages --> AskGenerate{Generate script<br/>without packages?}
    AskGenerate -->|Yes| JumpToStep5[Jump to Step 5:<br/>Configuration]
    AskGenerate -->|No| End([Exit])

    ShowWarning --> AskGenerate

    style Start fill:#F0E68C
    style PopularMode fill:#E6E6FA
    style SearchMode fill:#E6E6FA
    style SkipMode fill:#E6E6FA
    style NextStep fill:#F0E68C
    style JumpToStep5 fill:#F0E68C
    style End fill:#FFB6C1
```

---

## 6. Custom Flow - Step 4 (Version Selection)

This diagram shows version selection for each selected package.

```mermaid
flowchart TD
    Start([Step 4: Select Versions]) --> BeginLoop[For Each Selected Package]

    BeginLoop --> FetchVersions[Fetch Package Versions from NuGet<br/>Show spinner with package name]

    FetchVersions --> BuildChoices[Build Version Choices:<br/>- Latest Stable<br/>- Pre-release<br/>- Specific Versions list]

    BuildChoices --> CheckVersions{Found versions?}
    CheckVersions -->|No| ShowWarning[Show warning:<br/>No versions found<br/>Showing defaults only]
    CheckVersions -->|Yes| ShowChoices[Display Version Choices<br/>Page size: 12]
    ShowWarning --> ShowChoices

    ShowChoices --> UserSelects[User Selects Version]

    UserSelects --> MapChoice{Which option?}
    MapChoice -->|Latest Stable| SaveEmpty[Save: package -> empty string]
    MapChoice -->|Pre-release| SavePrerelease[Save: package -> '--prerelease']
    MapChoice -->|Specific Version| SaveVersion[Save: package -> version number]

    SaveEmpty --> ShowConfirm[Show confirmation message]
    SavePrerelease --> ShowConfirm
    SaveVersion --> ShowConfirm

    ShowConfirm --> MorePackages{More packages<br/>to process?}
    MorePackages -->|Yes| BeginLoop
    MorePackages -->|No| DisplaySummary[Display Final Selection Summary<br/>Show all packages with versions]

    DisplaySummary --> AskGenerate{Generate complete<br/>installation script?}
    AskGenerate -->|Yes| NextStep[Continue to Step 5:<br/>Configuration]
    AskGenerate -->|No| End([Exit])

    style Start fill:#F0E68C
    style NextStep fill:#F0E68C
    style End fill:#FFB6C1
```

---

## 7. Custom Flow - Step 5 (Project Configuration)

This diagram shows the detailed project configuration prompts.

```mermaid
flowchart TD
    Start([Step 5: Configure Project]) --> Section1[Template & Project Settings]

    Section1 --> PromptProject[Prompt: Project Name<br/>Default: MyProject]
    PromptProject --> AskSolution{Create solution file?<br/>Default: Yes}

    AskSolution -->|Yes| PromptSolution[Prompt: Solution Name<br/>Default: same as project]
    AskSolution -->|No| Section2
    PromptSolution --> Section2[Starter Kit Options]

    Section2 --> AskStarterKit{Include starter kit?<br/>Default: Yes}

    AskStarterKit -->|No| Section3
    AskStarterKit -->|Yes| SelectKit[Select Starter Kit:<br/>- clean<br/>- Articulate<br/>- Portfolio<br/>- LittleNorth.Igloo<br/>- Umbraco.BlockGrid.Example.Website<br/>- Umbraco.TheStarterKit<br/>- uSkinnedSiteBuilder]

    SelectKit --> FetchKitVersions[Fetch Starter Kit Versions<br/>Show spinner]
    FetchKitVersions --> SelectKitVersion[Select Version:<br/>- Latest Stable<br/>- Specific Versions]

    SelectKitVersion --> MapKitVersion{Which option?}
    MapKitVersion -->|Latest Stable| SetKitName[Set: StarterKitPackage = name only]
    MapKitVersion -->|Specific| SetKitVersion[Set: StarterKitPackage = 'name --version X.Y.Z']

    SetKitName --> Section3[Docker Options]
    SetKitVersion --> Section3

    Section3 --> AskDockerfile{Include Dockerfile?<br/>Default: No}
    AskDockerfile --> AskCompose{Include Docker Compose?<br/>Default: No}
    AskCompose --> SetDockerFlag[Set: CanIncludeDocker = true if either selected]

    SetDockerFlag --> Section4[Unattended Install Options]
    Section4 --> AskUnattended{Use unattended install?<br/>Default: Yes}

    AskUnattended -->|No| Section5
    AskUnattended -->|Yes| SelectDB[Select Database Type:<br/>- SQLite default<br/>- LocalDb<br/>- SQLServer<br/>- SQLAzure<br/>- SQLCE]

    SelectDB --> CheckDBType{SQL Server or<br/>SQL Azure?}
    CheckDBType -->|Yes| PromptConnStr[Prompt: Connection String]
    CheckDBType -->|No| PromptAdmin
    PromptConnStr --> PromptAdmin[Prompt: Admin User Friendly Name<br/>Default: Administrator]

    PromptAdmin --> PromptEmail[Prompt: Admin Email<br/>Default: admin@example.com]
    PromptEmail --> PromptPassword[Prompt: Admin Password<br/>Secret input<br/>Min 10 chars<br/>Default: 1234567890]

    PromptPassword --> Section5[Output Format Options]
    Section5 --> AskOneliner{Output as one-liner?<br/>Default: No}
    AskOneliner --> AskComments{Remove comments?<br/>Default: No}

    AskComments --> DisplaySummary[Display Configuration Summary<br/>Show all settings]
    DisplaySummary --> ConfirmGen{Confirm:<br/>Generate script?<br/>Default: Yes}

    ConfirmGen -->|No| Cancelled[Show: Cancelled]
    ConfirmGen -->|Yes| GenerateScript[Generate Script via API<br/>Show spinner]

    Cancelled --> End([Exit])
    GenerateScript --> DisplayScript[Display Generated Script]
    DisplayScript --> NextStep[Continue to Script Actions]

    style Start fill:#F0E68C
    style NextStep fill:#DDA0DD
    style End fill:#FFB6C1
    style Section1 fill:#FFFACD
    style Section2 fill:#FFFACD
    style Section3 fill:#FFFACD
    style Section4 fill:#FFFACD
    style Section5 fill:#FFFACD
```

---

## 8. Script Actions Flow

This diagram shows what happens after a script is generated (applies to both Default and Custom flows).

```mermaid
flowchart TD
    Start([Script Generated & Displayed]) --> AskAction{What to do with script?<br/>Choices:<br/>Run, Edit, Copy,<br/>Save, Start over}

    %% RUN PATH
    AskAction -->|Run| PromptDir[Prompt: Directory Path<br/>Default: current directory]
    PromptDir --> CheckPath{Path specified<br/>and different from<br/>current?}

    CheckPath -->|No| UseCurrentDir[Use Current Directory]
    CheckPath -->|Yes| ExpandPath[Validate & Expand Path]
    ExpandPath --> DirExists{Directory exists?}

    DirExists -->|Yes| RunScript[Execute Script in Directory<br/>Show output]
    DirExists -->|No| AskCreate{Create directory?}
    AskCreate -->|Yes| CreateDir[Create Directory<br/>Show confirmation]
    AskCreate -->|No| CancelRun[Show: Cancelled]
    CreateDir --> RunScript

    UseCurrentDir --> RunScript
    RunScript --> Complete([Complete Successfully])
    CancelRun --> End([Exit])

    %% EDIT PATH
    AskAction -->|Edit| CheckContext{Has script model<br/>and package versions?}
    CheckContext -->|Yes| EditWithDefaults[Re-run Step 5 Configuration<br/>with existing values as defaults]
    CheckContext -->|No| EditFromScratch[Re-run Custom Flow from start]

    EditWithDefaults --> BackToStep5[Return to Step 5 Flow]
    EditFromScratch --> BackToCustom[Return to Custom Flow Start]

    %% COPY PATH
    AskAction -->|Copy| CopyClipboard[Copy Script to Clipboard<br/>Show confirmation]
    CopyClipboard --> AskContinue{Do something else<br/>with script?<br/>Default: No}
    AskContinue -->|Yes| AskAction
    AskContinue -->|No| End

    %% SAVE PATH
    AskAction -->|Save| CheckModel{Has script model<br/>and package versions?}
    CheckModel -->|No| ShowCannotSave[Show: Cannot save template<br/>Configuration not available]
    ShowCannotSave --> End

    CheckModel -->|Yes| SaveFlow[Save as Template Flow]
    SaveFlow --> PromptName[Prompt: Template Name]
    PromptName --> PromptDesc[Prompt: Description<br/>Optional]
    PromptDesc --> PromptTags[Prompt: Tags<br/>Comma-separated, optional]

    PromptTags --> CreateTemplate[Create Template Object<br/>from ScriptModel]
    CreateTemplate --> TemplateExists{Template name<br/>already exists?}

    TemplateExists -->|No| SaveTemplate[Save Template to Disk<br/>~/.psw/templates/]
    TemplateExists -->|Yes| AskOverwrite{Overwrite existing?<br/>Default: No}
    AskOverwrite -->|No| CancelSave[Show: Cancelled]
    AskOverwrite -->|Yes| SaveTemplate

    SaveTemplate --> ShowSuccess[Show: Success message<br/>Show load command]
    ShowSuccess --> AskContinueSave{Do something else<br/>with script?<br/>Default: No}
    AskContinueSave -->|Yes| AskAction
    AskContinueSave -->|No| End
    CancelSave --> End

    %% START OVER PATH
    AskAction -->|Start over| ClearScreen[Clear screen<br/>Show: Starting over]
    ClearScreen --> RestartInteractive[Return to Interactive Mode Start]

    style Start fill:#DDA0DD
    style Complete fill:#90EE90
    style End fill:#FFB6C1
    style BackToStep5 fill:#F0E68C
    style BackToCustom fill:#FFE4B5
    style RestartInteractive fill:#87CEEB
```

---

## Summary of Flow Organization

### Flow Progression
1. **Main Entry Flow** → Determines which mode to use
2. **Interactive Mode - Main Flow** → Splits into Default or Custom path
3. **Default Script Flow** → Quick generation → Script Actions
4. **Custom Flow** → Steps 1-5 → Script Actions
   - **Step 1 & 2**: Template Selection
   - **Step 3**: Package Selection (3 modes)
   - **Step 4**: Version Selection for packages
   - **Step 5**: Project Configuration
5. **Script Actions Flow** → Run, Edit, Copy, Save, or Start Over

### Key Features

#### Ctrl+C Restart
- Entire interactive mode wrapped in try-catch for `OperationCanceledException`
- Clears screen, shows restart message, loops back to beginning
- Allows quick restart without exiting the application

#### Async Operations with Spinners
- Checking for updates
- Loading packages from API
- Fetching template versions
- Fetching package versions
- Generating scripts

#### Validation Points
- Project names (non-empty, valid characters)
- Solution names (if creating solution)
- Email format (for admin user)
- Password length (minimum 10 characters)
- Directory paths (for script execution)
- Template names (when saving)
- NuGet package ID format (when manually adding)

#### Caching Strategy
- Package list: 60 minutes in memory cache
- Package versions: 60 minutes in memory cache
- API responses: 1 hour TTL via ApiClient (configurable with `--no-cache`)

#### Error Handling
**Non-Fatal (Warnings):**
- Failed to load packages → Continue with limited selection
- Failed to fetch versions → Default to "Latest Stable"
- Version check failed → Continue without update notice

**Fatal (Exceptions):**
- Invalid project configuration (validation failures)
- API script generation failure
- Script execution errors

---

## Notes for Editing

When modifying these flows, consider:

1. **Step Numbers**: Hard-coded in UI messages - if you change step order, update the strings
2. **Default Values**: Default script model must match website defaults
3. **Package Format**:
   - Latest: `PackageName`
   - Prerelease: `PackageName --prerelease`
   - Specific: `PackageName|Version`
4. **Starter Kit Format**:
   - Latest: `StarterKitName`
   - Specific: `StarterKitName --version X.Y.Z`
5. **Storage Locations**:
   - Templates: `~/.psw/templates/`
   - History: `~/.psw/history/`
6. **Template Version**: Empty string means latest stable, `--prerelease` means latest prerelease

---

## How to Use This Documentation

1. **Review each diagram** to understand the current flow
2. **Identify changes** you want to make to each specific process
3. **Edit the mermaid diagrams** to reflect your desired flow
4. **Update the notes** to document any new behavior or requirements
5. **Share the updated document** for implementation
