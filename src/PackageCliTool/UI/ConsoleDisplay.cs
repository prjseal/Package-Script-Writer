using PackageCliTool.Configuration;
using Spectre.Console;
using System.Reflection;
using System.Text;

namespace PackageCliTool.UI;

/// <summary>
/// Handles console display for banners, help, and version information
/// </summary>
public static class ConsoleDisplay
{
    /// <summary>
    /// Displays the welcome banner using Spectre.Console
    /// </summary>
    public static void DisplayWelcomeBanner()
    {
        AnsiConsole.Write(
            new FigletText("PSW CLI")
                .LeftJustified()
                .Color(Color.Blue));

        AnsiConsole.MarkupLine("[dim]Package Script Writer - Interactive CLI[/]");
        AnsiConsole.MarkupLine("[dim]By Paul Seal[/]");
        AnsiConsole.MarkupLine("[dim]Press Ctrl+C at any time to start over[/]");
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Displays help information with all available flags
    /// </summary>
    public static void DisplayHelp()
    {
        AnsiConsole.Write(
            new FigletText("PSW CLI")
                .LeftJustified()
                .Color(Color.Blue));

        AnsiConsole.MarkupLine("[dim]Package Script Writer - Interactive CLI[/]");
        AnsiConsole.MarkupLine("[dim]By Paul Seal[/]\n");

        var helpPanel = new Panel(
            @"[bold yellow]USAGE:[/]
  psw [[options]]
  psw template <command> [[options]]
  psw history <command> [[options]]
  psw versions
  psw list-options [[category]]

[bold yellow]MAIN OPTIONS:[/]
  [green]-d, --default[/]                 Generate a default script with minimal configuration
  [green]-p, --packages[/] <packages>     [dim](string)[/] Comma-separated packages with optional versions
                                   Format: ""Package1|Version1,Package2""
                                   Example: ""uSync|17.0.0,Umbraco.Forms""
  [green]-t, --template-package[/] <pkg>  [dim](string)[/] Template package with optional version
                                   Format: ""PackageName|Version"" or just ""PackageName""
                                   Example: ""Umbraco.Templates|17.0.3""
  [green]    --template-version[/] <ver>  [dim](string)[/] Template version (alternative to pipe syntax)
  [green]-n, --project-name[/] <name>     [dim](string, default: MyProject)[/] Project name
  [green]-s, --solution[/] <name>         [dim](string)[/] Solution name (enables solution file creation)
  [green]-k, --starter-kit[/] <pkg>       [dim](string)[/] Starter kit: clean, Articulate, Portfolio, etc.
                                   Format: ""PackageName|Version"" or just ""PackageName""
  [green]    --dockerfile[/]              Include Dockerfile in generated script
  [green]    --docker-compose[/]          Include Docker Compose file in generated script
  [green]-da, --delivery-api[/]           Enable Content Delivery API
  [green]-u, --unattended-defaults[/]     Use unattended install with defaults (SQLite, admin@example.com)
  [green]    --database-type[/] <type>    [dim](enum: SQLite, LocalDb, SQLServer, SQLAzure, SQLCE)[/]
                                   Database type. Implies unattended install.
                                   [dim]Note: SQLServer/SQLAzure require --connection-string[/]
  [green]    --connection-string[/] <str> [dim](string)[/] Database connection string
  [green]    --admin-name[/] <name>       [dim](string, default: Administrator)[/] Admin friendly name
  [green]    --admin-email[/] <email>     [dim](string, default: admin@example.com)[/] Admin email
  [green]    --admin-password[/] <pwd>    [dim](string, default: 1234567890)[/] Admin password (min 10 chars)
  [green]-o, --oneliner[/]                Output script as one-liner
  [green]-r, --remove-comments[/]         Remove comments from generated script
  [green]    --include-prerelease[/]      Include prerelease package versions
  [green]    --auto-run[/]                Automatically execute the generated script
  [green]    --no-build[/]               Skip 'dotnet run' from the generated script (build only)
  [green]    --run-dir[/] <directory>     [dim](string)[/] Directory to run script in
  [green]    --save-only[/]               Save script to file (via --output-file) and exit without prompts
  [green]    --output-file[/] <file>      [dim](string)[/] Output file path for saving the generated script
  [green]    --community-template[/] <n>  [dim](string)[/] Load community template by name, or 'list'

[bold yellow]OUTPUT & AI AGENT OPTIONS:[/]
  [green]    --output[/] <format>         [dim](enum: json, plain)[/] Output format for machine consumption
  [green]    --script-only[/]             Output only the raw script text, no decoration
  [green]    --no-interaction[/]          Suppress all interactive prompts (fail if input needed)
  [green]    --dry-run[/]                 Validate inputs and show config without generating script
  [green]    --help-json[/]               Show all commands and options as structured JSON

[bold yellow]GENERAL OPTIONS:[/]
  [green]-h, --help[/]                    Show this help information
  [green]-v, --version[/]                 Show version information
  [green]    --verbose[/]                 Enable verbose logging mode
  [green]    --clear-cache[/]             Clear all cached API responses

[bold yellow]TEMPLATE COMMANDS:[/]
  [green]psw template save[/] <name>      Save current configuration as a template
  [green]psw template load[/] <name>      Load and execute a template
  [green]psw template list[/]             List all available templates
  [green]psw template delete[/] <name>    Delete a template
  [green]psw template export[/] <name>    Export template to file
  [green]psw template import[/] <file>    Import template from file
  [green]psw template validate[/] <file>  Validate template file

[bold yellow]TEMPLATE OPTIONS:[/]
  [green]    --template-description[/] <desc> Template description
  [green]    --template-tags[/] <tags>   Comma-separated tags
  [green]    --template-file[/] <path>   Template file path

[bold yellow]HISTORY COMMANDS:[/]
  [green]psw history list[/]              List recent script generation history
  [green]psw history rerun[/] <#>         Regenerate and re-run a script from history
  [green]psw history delete[/] <#>        Delete a history entry
  [green]psw history clear[/]             Clear all history

[bold yellow]HISTORY OPTIONS:[/]
  [green]    --history-limit[/] <count>  Number of entries to show (default: 10)

[bold yellow]VERSIONS COMMAND:[/]
  [green]psw versions[/]                 Display Umbraco versions table with support lifecycle information

[bold yellow]EXAMPLES:[/]
  Generate default script:
    [cyan]psw --default[/]

  Generate custom script with packages (latest versions):
    [cyan]psw --packages ""uSync,Umbraco.Forms"" --project-name MyProject[/]

  Generate script with specific package versions:
    [cyan]psw --packages ""uSync|17.0.0,clean|7.0.1"" --project-name MyProject[/]

  Mixed: some with versions, some without:
    [cyan]psw -p ""uSync|17.0.0,Umbraco.Forms"" -n MyProject[/]

  Specify template package and version:
    [cyan]psw -t ""Umbraco.Templates|17.0.2"" -n MyProject[/]

  Full configuration example:
    [cyan]psw -p ""uSync|17.0.0"" -n MyProject -s MySolution -u --database-type SQLite --admin-email admin@test.com --admin-password MyPass123! --auto-run[/]

  Interactive mode (no flags):
    [cyan]psw[/]

[bold yellow]AUTOMATION EXAMPLES:[/]
  Auto-run but skip 'dotnet run' (install + build only):
    [cyan]psw -d -n MyProject -s MyProject -u --database-type SQLite --admin-email admin@test.com --admin-password MyPass123! --auto-run --no-build[/]

  Save script to file without interactive prompts:
    [cyan]psw -d -n MyProject -s MyProject -u --database-type SQLite --admin-email admin@test.com --admin-password MyPass123! --output-file install.sh --save-only[/]

[bold yellow]TEMPLATE EXAMPLES:[/]
  Save current configuration as template:
    [cyan]psw template save my-blog --template-description ""My blog setup"" --template-tags ""blog,umbraco14""[/]

  List all templates:
    [cyan]psw template list[/]

  Load and use a template:
    [cyan]psw template load my-blog[/]

  Load template with overrides:
    [cyan]psw template load my-blog -n NewBlog -s MySolution --auto-run[/]

  Export template to file:
    [cyan]psw template export my-blog --template-file my-blog.yaml[/]

  Import template from file:
    [cyan]psw template import my-blog.yaml[/]

[bold yellow]HISTORY EXAMPLES:[/]
  List recent scripts:
    [cyan]psw history list[/]

  Re-run a previous script:
    [cyan]psw history rerun 1[/]

  Clear all history:
    [cyan]psw history clear[/]

[bold yellow]CACHE EXAMPLES:[/]
  Clear the cache:
    [cyan]psw --clear-cache[/]

  Clear cache and generate fresh script:
    [cyan]psw --clear-cache --packages ""uSync"" --project-name MyProject[/]

[bold yellow]VERSIONS EXAMPLES:[/]
  Display Umbraco versions table:
    [cyan]psw versions[/]

[bold yellow]AI AGENT / AUTOMATION EXAMPLES:[/]
  Generate script as JSON (for programmatic use):
    [cyan]psw --default --output json[/]

  Generate script with no decoration (pipe-friendly):
    [cyan]psw --default --script-only[/]

  Validate config without generating (dry run):
    [cyan]psw --dry-run -p ""uSync|17.0.0"" --database-type SQLite[/]

  Get help as JSON (for AI tool discovery):
    [cyan]psw --help-json[/]

  List valid option values as JSON:
    [cyan]psw list-options --output json[/]

  List valid database types:
    [cyan]psw list-options database-types[/]

  Non-interactive mode (no prompts):
    [cyan]psw --default --no-interaction --script-only[/]

  Get version as plain text:
    [cyan]psw --version --output plain[/]")
            .Header("[bold blue]Package Script Writer Help[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Blue)
            .Padding(1, 1);

        AnsiConsole.Write(helpPanel);
    }

    /// <summary>
    /// Displays help information for template commands
    /// </summary>
    public static void DisplayTemplateHelp()
    {
        var helpPanel = new Panel(
            @"[bold yellow]USAGE:[/]
  psw template <command> [[options]]

[bold yellow]TEMPLATE COMMANDS:[/]
  [green]save[/] <name>          Save current configuration as a template
  [green]load[/] <name>          Load and execute a template
  [green]list[/]                 List all available templates
  [green]delete[/] <name>        Delete a template
  [green]export[/] <name>        Export template to file
  [green]import[/] <file>        Import template from file
  [green]validate[/] <file>      Validate template file

[bold yellow]TEMPLATE OPTIONS:[/]
  [green]    --template-description[/] <desc> Template description
  [green]    --template-tags[/] <tags>   Comma-separated tags
  [green]    --template-file[/] <path>   Template file path

[bold yellow]EXAMPLES:[/]
  Save current configuration as template:
    [cyan]psw template save my-blog --template-description ""My blog setup"" --template-tags ""blog,umbraco14""[/]

  List all templates:
    [cyan]psw template list[/]

  Load and use a template:
    [cyan]psw template load my-blog[/]

  Load template with overrides:
    [cyan]psw template load my-blog -n NewBlog -s MySolution --auto-run[/]

  Export template to file:
    [cyan]psw template export my-blog --template-file my-blog.yaml[/]

  Import template from file:
    [cyan]psw template import my-blog.yaml[/]")
            .Header("[bold blue]Template Command Help[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Blue)
            .Padding(1, 1);

        AnsiConsole.Write(helpPanel);
    }

    /// <summary>
    /// Displays help information for history commands
    /// </summary>
    public static void DisplayHistoryHelp()
    {
        var helpPanel = new Panel(
            @"[bold yellow]USAGE:[/]
  psw history <command> [[options]]

[bold yellow]HISTORY COMMANDS:[/]
  [green]list[/]                  List recent script generation history
  [green]rerun[/] <#>             Regenerate and re-run a script from history
  [green]delete[/] <#>            Delete a history entry
  [green]clear[/]                 Clear all history

[bold yellow]HISTORY OPTIONS:[/]
  [green]    --history-limit[/] <count>  Number of entries to show (default: 10)

[bold yellow]EXAMPLES:[/]
  List recent scripts:
    [cyan]psw history list[/]

  Re-run a previous script:
    [cyan]psw history rerun 1[/]

  Clear all history:
    [cyan]psw history clear[/]")
            .Header("[bold blue]History Command Help[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Blue)
            .Padding(1, 1);

        AnsiConsole.Write(helpPanel);
    }

    /// <summary>
    /// Displays version information
    /// </summary>
    public static void DisplayVersion()
    {
        AnsiConsole.Write(
            new FigletText("PSW CLI")
                .LeftJustified()
                .Color(Color.Blue));
        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
        AnsiConsole.MarkupLine($"[bold]Version:[/] {currentVersion}");
        AnsiConsole.MarkupLine("[dim]Package Script Writer CLI Tool[/]");
        AnsiConsole.MarkupLine("[dim]By Paul Seal[/]");
        AnsiConsole.MarkupLine($"[dim]https://github.com/prjseal/Package-Script-Writer[/]");
    }

    /// <summary>
    /// Displays the generated script in a panel
    /// </summary>

public static void DisplayGeneratedScript(string script, string title = "Generated Installation Script")
    {
        // Colors:
        // Comments: #41535b -> rgb(65,83,91)
        // Quotes:   #55b5db -> rgb(85,181,219)
        const string CommentColorOpen = "[rgb(65,83,91)]";
        const string QuoteColorOpen   = "[rgb(85,181,219)]";
        const string CloseTag         = "[/]";

        static string ColorizeLine(string line)
        {
            var sb = new StringBuilder(line.Length + 32);

            // Base coloring: only for comment lines. Non-comment lines use the terminal's default color.
            bool isComment = line.TrimStart().StartsWith("#");
            bool baseOpened = false;

            if (isComment)
            {
                sb.Append(CommentColorOpen);
                baseOpened = true;
            }

            bool inQuote = false;
            char quoteChar = '\0';
            char prev = '\0';

            var chunk = new StringBuilder();

            void FlushChunk()
            {
                if (chunk.Length > 0)
                {
                    sb.Append(Markup.Escape(chunk.ToString()));
                    chunk.Clear();
                }
            }

            foreach (char c in line)
            {
                bool isQuoteCandidate = c == '"' || c == '\'';
                bool escaped = prev == '\\';

                if (isQuoteCandidate && !escaped)
                {
                    // We reached a quote character
                    FlushChunk();

                    if (!inQuote)
                    {
                        // Start quoted section (include opening quote)
                        inQuote = true;
                        quoteChar = c;
                        sb.Append(QuoteColorOpen);
                        sb.Append(Markup.Escape(c.ToString()));
                    }
                    else if (quoteChar == c)
                    {
                        // End quoted section (include closing quote)
                        sb.Append(Markup.Escape(c.ToString()));
                        sb.Append(CloseTag); // back to base (comment color if opened) or default
                        inQuote = false;
                        quoteChar = '\0';
                    }
                    else
                    {
                        // Different quote while inside a quote -> treat as normal char
                        chunk.Append(c);
                    }
                }
                else
                {
                    chunk.Append(c);
                }

                prev = c;
            }

            // Flush remaining characters
            FlushChunk();

            // If line ends inside a quote, close the quote color for valid markup
            if (inQuote)
                sb.Append(CloseTag);

            // Close base color only if we opened it (i.e., comment line)
            if (baseOpened)
                sb.Append(CloseTag);

            return sb.ToString();
        }

        var lines = (script ?? string.Empty).Replace("\r\n", "\n").Split('\n');
        var markup = new StringBuilder(script?.Length ?? 0 + lines.Length * 8);
        foreach (var line in lines)
            markup.AppendLine(ColorizeLine(line));

        AnsiConsole.WriteLine();
        var panel = new Panel(new Markup(markup.ToString()))
            .Header($"[bold cyan]{Markup.Escape(title)}[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Grey)
            .Padding(4, 2);

        AnsiConsole.Write(panel);
    }

    /// <summary>
    /// Displays the Umbraco versions table
    /// </summary>
    public static void DisplayUmbracoVersions(PSW.Shared.Configuration.PSWConfig pswConfig)
    {
        AnsiConsole.Write(
            new FigletText("Umbraco Versions")
                .LeftJustified()
                .Color(Color.Blue));

        AnsiConsole.MarkupLine("[dim]This is a handy table to help you work out which version of Umbraco to use.[/]");
        AnsiConsole.MarkupLine("[dim]Find out more: https://umbraco.com/products/knowledge-center/long-term-support-and-end-of-life/[/]\n");

        if (pswConfig?.UmbracoVersions == null || !pswConfig.UmbracoVersions.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No Umbraco version data available.[/]");
            return;
        }

        var oneYearFromNow = DateTime.UtcNow.AddYears(1);
        const string dateFormat = "MMM dd, yyyy";

        // Create the versions table
        var table = new Table();
        table.Border(TableBorder.Horizontal);
        table.BorderColor(Color.Grey);

        // Add columns
        table.AddColumn(new TableColumn("[bold]Version[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Release Date[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Release Type[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Support Phase[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Security Phase[/]").Centered());
        table.AddColumn(new TableColumn("[bold]End of Life[/]").Centered());

        // Add rows
        foreach (var version in pswConfig.UmbracoVersions)
        {
            var isLTS = version.ReleaseType == "LTS";
            var isEndOfLife = DateTime.UtcNow >= version.EndOfLife;
            var isSTS = !isLTS;
            var willEOLInLessThanAYear = !isEndOfLife && oneYearFromNow > version.EndOfLife;
            var isFutureRelease = version.ReleaseDate > DateTime.UtcNow;

            // Determine color based on status
            var rowColor = "white";
            if (isEndOfLife)
            {
                rowColor = "red";
            }
            else if ((isSTS && !isFutureRelease) || willEOLInLessThanAYear)
            {
                rowColor = "yellow";
            }
            else if (isFutureRelease)
            {
                rowColor = "grey";
            }
            else
            {
                rowColor = "green";
            }

            // Version column (with URL if available)
            var versionText = !string.IsNullOrWhiteSpace(version.Url)
                ? $"[{rowColor}][link={version.Url}]{version.Version}[/][/]"
                : $"[{rowColor}]{version.Version}[/]";

            // Apply color to all cells in the row
            table.AddRow(
                versionText,
                $"[{rowColor}]{version.ReleaseDate.ToString(dateFormat)}[/]",
                isLTS ? $"[{rowColor}][bold]{version.ReleaseType}[/][/]" : $"[{rowColor}]{version.ReleaseType ?? ""}[/]",
                $"[{rowColor}]{version.SupportPhase?.ToString(dateFormat) ?? ""}[/]",
                $"[{rowColor}]{version.SecurityPhase?.ToString(dateFormat) ?? ""}[/]",
                $"[{rowColor}]{version.EndOfLife.ToString(dateFormat)}[/]"
            );
        }

        AnsiConsole.Write(table);

        // Display key
        AnsiConsole.WriteLine();
        var keyTable = new Table();
        keyTable.Border(TableBorder.Rounded);
        keyTable.BorderColor(Color.Grey);
        keyTable.AddColumn(new TableColumn("[bold]Key[/]"));

        keyTable.AddRow("[red]End of Life[/]");
        keyTable.AddRow("[yellow]Standard Term Support (STS) or End of Life within 1 year from today[/]");
        keyTable.AddRow("[green]Long Term Support (LTS) and more than 1 year before end of life[/]");
        keyTable.AddRow("[grey]Future release[/]");

        AnsiConsole.Write(keyTable);
    }

}
