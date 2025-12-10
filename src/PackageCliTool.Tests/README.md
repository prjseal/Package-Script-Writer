# PackageCliTool.Tests

Unit tests for the Package Script Writer CLI tool.

## Overview

This test project provides comprehensive unit tests for the CLI tool functionality, with a focus on command-line argument parsing and flag handling.

## Test Structure

### CommandLineOptionsTests.cs
Comprehensive tests for the `CommandLineOptions.Parse()` method covering:
- Individual flag parsing (all 25+ flags)
- Short and long flag variants (`-h` vs `--help`)
- Flag combinations
- HasAnyOptions() logic
- Edge cases

### TemplatePackageFlagTests.cs
Specific tests for the `--template-package` flag functionality:
- Optional template package behavior
- Community template support
- Integration with other flags
- Edge cases and scenarios

## Running Tests

### Command Line

```bash
# Run all tests
dotnet test src/PackageCliTool.Tests

# Run tests with detailed output
dotnet test src/PackageCliTool.Tests --verbosity normal

# Run tests with coverage
dotnet test src/PackageCliTool.Tests --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test src/PackageCliTool.Tests --filter "FullyQualifiedName~CommandLineOptionsTests"

# Run specific test
dotnet test src/PackageCliTool.Tests --filter "FullyQualifiedName~Parse_WithTemplatePackageFlag_SetsTemplatePackageName"
```

### Visual Studio
1. Open Test Explorer (Test → Test Explorer)
2. Click "Run All" to run all tests
3. Right-click individual tests to run/debug specific tests

### VS Code
1. Install C# Dev Kit extension
2. Click the beaker icon in the sidebar
3. Run tests from the Test Explorer

## Test Coverage

Current test coverage includes:

**Command-Line Parsing:**
- ✅ Help and version flags
- ✅ Default flag
- ✅ Template package and version flags
- ✅ Package selection and versioning
- ✅ Project and solution configuration
- ✅ Starter kit options
- ✅ Docker options
- ✅ Unattended install configuration
- ✅ Output formatting options
- ✅ Execution options
- ✅ Cache options
- ✅ Verbose logging

**Template Package Flag Specific:**
- ✅ Optional behavior (can be omitted)
- ✅ Umbraco.Templates support
- ✅ Community template support
- ✅ Integration with version flag
- ✅ Complex command parsing
- ✅ Edge cases

## Test Examples

### Example 1: Basic Flag Parsing
```csharp
[Fact]
public void Parse_WithTemplatePackageFlag_SetsTemplatePackageName()
{
    // Arrange
    var args = new[] { "--template-package", "Umbraco.Templates" };

    // Act
    var options = CommandLineOptions.Parse(args);

    // Assert
    options.TemplatePackageName.Should().Be("Umbraco.Templates");
}
```

### Example 2: Complex Scenario
```csharp
[Fact]
public void Parse_WithMultipleFlags_ParsesAllCorrectly()
{
    // Arrange
    var args = new[]
    {
        "--template-package", "Umbraco.Templates",
        "-t", "14.3.0",
        "-p", "uSync|17.0.0",
        "-n", "MyProject"
    };

    // Act
    var options = CommandLineOptions.Parse(args);

    // Assert - All flags parsed correctly
}
```

### Example 3: Theory-Based Tests
```csharp
[Theory]
[InlineData("-h")]
[InlineData("--help")]
public void Parse_WithHelpFlag_SetsShowHelp(string helpFlag)
{
    // Tests both short and long versions
}
```

## Adding New Tests

When adding new CLI flags:

1. Add parsing test in `CommandLineOptionsTests.cs`:
   ```csharp
   [Fact]
   public void Parse_WithNewFlag_SetsProperty()
   {
       var args = new[] { "--new-flag", "value" };
       var options = CommandLineOptions.Parse(args);
       options.NewProperty.Should().Be("value");
   }
   ```

2. Add integration test showing the flag in combination with others

3. Add edge case tests if applicable

4. Update `HasAnyOptions()` test if the new flag should be included

## Continuous Integration

Tests are automatically run by GitHub Actions on:
- Pull requests
- Pushes to main branch
- Release builds

## Test Dependencies

- **xUnit** - Test framework
- **FluentAssertions** - Assertion library for readable tests
- **coverlet.collector** - Code coverage collection

## Best Practices

1. **Arrange-Act-Assert** - All tests follow AAA pattern
2. **Descriptive Names** - Test names clearly describe what they test
3. **Single Responsibility** - Each test verifies one thing
4. **Theory Tests** - Use `[Theory]` for testing multiple inputs
5. **Readable Assertions** - Use FluentAssertions for clear error messages

## Troubleshooting

### Tests Not Running
- Ensure .NET 10.0 SDK is installed
- Restore NuGet packages: `dotnet restore`
- Clean and rebuild: `dotnet clean && dotnet build`

### Test Failures
- Check that CommandLineOptions.cs hasn't changed
- Verify flag names match current implementation
- Check for breaking changes in the CLI

### Coverage Issues
- Ensure coverlet.collector is installed
- Run with coverage: `dotnet test --collect:"XPlat Code Coverage"`
- View coverage reports in `TestResults` directory

## Future Test Coverage

Planned additions:
- Integration tests for CliModeWorkflow
- Tests for script generation with different flag combinations
- Template loading and validation tests
- Error handling and validation tests
- History and cache functionality tests

## Contributing

When contributing tests:
1. Follow existing naming conventions
2. Add documentation for complex test scenarios
3. Ensure tests are independent and can run in any order
4. Add both positive and negative test cases
5. Update this README with new test categories
