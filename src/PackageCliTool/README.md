# Package CLI Tool

An interactive command-line interface for the Package Script Writer API, built with .NET 10.0 and Spectre.Console.

## Features

- 🎨 **Beautiful CLI Interface** - Built with Spectre.Console for a rich terminal experience
- 📦 **Package Selection** - Multi-select from popular Umbraco packages or add custom ones
- 🔢 **Version Selection** - Choose specific versions for each selected package
- ⚡ **Progress Indicators** - Spinners and progress displays during API calls
- 📄 **Script Generation** - Generate complete installation scripts
- 💾 **Export Scripts** - Save generated scripts to files

## Requirements

- .NET 10.0 SDK or later
- Internet connection (to access the Package Script Writer API)

## Installation

### Option 1: Build and Run from Source

1. Navigate to the project directory:
   ```bash
   cd src/PackageCliTool
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Run the CLI tool:
   ```bash
   dotnet run
   ```

### Option 2: Open in Visual Studio

1. Open `PackageCliTool.sln` in Visual Studio
2. Press F5 to build and run

### Option 3: Build and Install Globally

1. Build the project:
   ```bash
   dotnet build -c Release
   ```

2. Pack as a .NET tool (optional):
   ```bash
   dotnet pack -c Release
   ```

3. Install globally:
   ```bash
   dotnet tool install --global --add-source ./nupkg PackageCliTool
   ```

## Usage

### Basic Workflow

1. **Start the CLI**:
   ```bash
   dotnet run
   ```

2. **Select Packages**:
   - Use arrow keys to navigate
   - Press `Space` to select/deselect packages
   - Press `Enter` to confirm selection
   - You can also select "Add custom package..." to enter a package name manually

3. **Select Versions**:
   - For each selected package, choose a version from the list
   - The tool fetches versions from the API in real-time

4. **Review Selection**:
   - View your selected packages and versions in a formatted table

5. **Generate Script** (Optional):
   - Choose whether to generate a complete installation script
   - Enter project name and Umbraco template version
   - View the generated script
   - Optionally save it to a file

### Example Session

```
 ____            _                           ____ _     ___   _____           _
|  _ \ __ _  ___| | ____ _  __ _  ___       / ___| |   |_ _| |_   _|__   ___ | |
| |_) / _` |/ __| |/ / _` |/ _` |/ _ \     | |   | |    | |    | |/ _ \ / _ \| |
|  __/ (_| | (__|   < (_| | (_| |  __/     | |___| |___ | |    | | (_) | (_) | |
|_|   \__,_|\___|_|\_\__,_|\__, |\___|      \____|_____|___|   |_|\___/ \___/|_|
                           |___/

Package Script Writer - Interactive CLI

Step 1: Select Packages

Select one or more packages (use Space to select, Enter to confirm):
  [ ] Umbraco.Community.BlockPreview
  [X] Diplo.GodMode
  [X] uSync
  [ ] Umbraco.Community.Contentment
  ...

✓ Selected Diplo.GodMode version 3.0.3
✓ Selected uSync version 12.0.0

Step 3: Final Selection

┌───────────────────────────┬──────────────────┐
│      Package Name         │ Selected Version │
├───────────────────────────┼──────────────────┤
│ Diplo.GodMode             │      3.0.3       │
│ uSync                     │     12.0.0       │
└───────────────────────────┴──────────────────┘

Would you like to generate a complete installation script? (y/n): y
```

## Project Structure

```
PackageCliTool/
├── PackageCliTool.sln          # Solution file
├── PackageCliTool.csproj       # Project file (.NET 10.0)
├── Program.cs                  # Main application code
└── README.md                   # This file
```

## Code Structure

### Main Components

1. **Program Class** - Entry point and orchestration
   - `Main()` - Application entry point
   - `DisplayWelcomeBanner()` - Shows ASCII art banner
   - `SelectPackagesAsync()` - Multi-select package prompt
   - `SelectVersionsForPackagesAsync()` - Version selection for each package
   - `DisplayFinalSelection()` - Shows selected packages in a table
   - `GenerateAndDisplayScriptAsync()` - Generates installation script

2. **ApiClient Class** - API communication
   - `GetPackageVersionsAsync()` - Fetches package versions
   - `GenerateScriptAsync()` - Generates installation script

3. **Data Models** - Request/Response DTOs
   - `PackageVersionRequest/Response`
   - `ScriptRequest/Response`
   - `ScriptModel`

### Key Features in Code

#### Async/Await Pattern
All API calls use async/await for non-blocking operations:
```csharp
var versions = await apiClient.GetPackageVersionsAsync(package, includePrerelease: false);
```

#### Spectre.Console Integration
- **MultiSelectionPrompt** - For selecting multiple packages
- **SelectionPrompt** - For selecting single version
- **Status/Spinner** - For showing progress during API calls
- **Table** - For displaying results
- **Panel** - For showing generated scripts
- **FigletText** - For ASCII art banner

#### Error Handling
Comprehensive try-catch blocks with user-friendly error messages:
```csharp
try
{
    // API operations
}
catch (Exception ex)
{
    AnsiConsole.MarkupLine($"[red]✗ Error: {ex.Message}[/]");
}
```

## API Integration

The tool connects to the Package Script Writer API:
- **Base URL**: `https://psw.codeshare.co.uk`
- **Endpoints**:
  - `POST /api/scriptgeneratorapi/getpackageversions` - Get package versions
  - `POST /api/scriptgeneratorapi/generatescript` - Generate installation script

## Configuration

### Popular Packages List
The CLI includes a predefined list of popular Umbraco packages in `Program.cs`:

```csharp
private static readonly List<string> PopularPackages = new()
{
    "Umbraco.Community.BlockPreview",
    "Diplo.GodMode",
    "uSync",
    "Umbraco.Community.Contentment",
    // ... more packages
};
```

You can customize this list to include your preferred packages.

### API Timeout
Default timeout is 30 seconds. Modify in `ApiClient` constructor:
```csharp
_httpClient = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(30)
};
```

## Troubleshooting

### Package Not Found
If a package cannot be found:
- Verify the package name is correct
- Check if the package exists on NuGet.org
- Ensure you have internet connectivity

### API Connection Issues
If you cannot connect to the API:
- Check your internet connection
- Verify the API is accessible: https://psw.codeshare.co.uk/api/docs
- Check firewall settings

### .NET Version Issues
Ensure you have .NET 10.0 SDK installed:
```bash
dotnet --version
```

If not, download from: https://dotnet.microsoft.com/download

## Development

### Adding New Features

The code is modularized for easy extension:

1. **Add new UI prompts** - Use Spectre.Console prompts
2. **Add new API endpoints** - Extend `ApiClient` class
3. **Customize display** - Modify display methods

### Code Comments

The code includes extensive comments explaining:
- Method purposes
- Parameter descriptions
- Key logic decisions
- API interactions

## Dependencies

- **Spectre.Console** (v0.49.1) - Terminal UI framework
  - Rich formatting and colors
  - Interactive prompts
  - Progress indicators
  - Tables and panels

## License

This project is part of the Package Script Writer application.

## Links

- **Main Repository**: https://github.com/prjseal/Package-Script-Writer
- **API Documentation**: https://github.com/prjseal/Package-Script-Writer/blob/main/.github/api-reference.md
- **Swagger UI**: https://psw.codeshare.co.uk/api/docs

## Support

For issues or questions:
1. Check the API documentation
2. Review the Swagger UI for endpoint details
3. Open an issue on the main repository
