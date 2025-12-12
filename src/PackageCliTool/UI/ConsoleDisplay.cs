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

[bold yellow]MAIN OPTIONS:[/]
  [green]    --admin-email[/] <email>     Admin email for unattended install
  [green]    --admin-name[/] <name>       Admin user friendly name for unattended install
  [green]    --admin-password[/] <pwd>    Admin password for unattended install
  [green]    --auto-run[/]                Automatically run the generated script
  [green]    --clear-cache[/]             Clear all cached API responses
  [green]    --connection-string[/] <str> Connection string (for SQLServer/SQLAzure)
  [green]    --database-type[/] <type>    Database type (SQLite, LocalDb, SQLServer, SQLAzure, SQLCE)
  [green]-d, --default[/]                 Generate a default script with minimal configuration
  [green]    --dockerfile[/]              Include Dockerfile in generated script
  [green]    --docker-compose[/]          Include Docker Compose file in generated script
  [green]-h, --help[/]                    Show this help information
  [green]    --include-prerelease[/]      Include prerelease package versions
  [green]-k, --starter-kit[/] <package>   Starter kit package name
  [green]-n, --project-name[/] <name>     Project name (default: MyProject)
  [green]    --no-cache[/]                Disable API response caching (bypass cache)
  [green]-o, --oneliner[/]                Output script as one-liner
  [green]-p, --packages[/] <packages>     Comma-separated list of packages with optional versions
                                   Format: ""Package1|Version1,Package2|Version2""
                                   Or just package names: ""uSync,Umbraco.Forms"" (uses latest)
                                   Example: ""uSync|17.0.0,clean|7.0.1""
  [green]-r, --remove-comments[/]         Remove comments from generated script
  [green]    --run-dir[/] <directory>     Directory to run script in
  [green]-s, --solution[/]                Solution name
  [green]    --starter-kit-package[/] <pkg> Starter kit package name
  [green]-t, --template-package[/] <package>     Comma-separated list of packages with optional version
                                   Format: ""Package|Version""
                                   Or just template name: ""Umbraco.Templates"" (uses latest)
                                   Example: ""Umbraco.Templates|17.0.3""
  [green]-u, --unattended-defaults[/]     Use unattended install defaults
  [green]    --update-packages[/]         Update package list cache from marketplace
  [green]-v, --version[/]                 Show version information
  [green]    --verbose[/]                 Enable verbose logging mode

[bold yellow]TEMPLATE COMMANDS:[/]
  [green]psw template save[/] <name>      Save current configuration as a template
  [green]psw template load[/] <name>      Load and execute a template
  [green]psw template list[/]             List all available templates
  [green]psw template show[/] <name>      Show template details
  [green]psw template delete[/] <name>    Delete a template
  [green]psw template export[/] <name>    Export template to file
  [green]psw template import[/] <file>    Import template from file
  [green]psw template validate[/] <file>  Validate template file

[bold yellow]TEMPLATE OPTIONS:[/]
  [green]    --template-description[/] <desc> Template description
  [green]    --template-tags[/] <tags>   Comma-separated tags
  [green]    --template-file[/] <path>   Template file path
  [green]    --set[/] <key=value>        Override template values

[bold yellow]HISTORY COMMANDS:[/]
  [green]psw history list[/]              List recent script generation history
  [green]psw history show[/] <#>          Show details of a history entry
  [green]psw history rerun[/] <#>         Regenerate and re-run a script from history
  [green]psw history delete[/] <#>        Delete a history entry
  [green]psw history clear[/]             Clear all history
  [green]psw history stats[/]             Show history statistics

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

[bold yellow]TEMPLATE EXAMPLES:[/]
  Save current configuration as template:
    [cyan]psw template save my-blog --template-description ""My blog setup"" --template-tags ""blog,umbraco14""[/]

  List all templates:
    [cyan]psw template list[/]

  Load and use a template:
    [cyan]psw template load my-blog[/]

  Load template with overrides:
    [cyan]psw template load my-blog --project-name NewBlog --set AutoRun=true[/]

  Export template to file:
    [cyan]psw template export my-blog --template-file my-blog.yaml[/]

  Import template from file:
    [cyan]psw template import my-blog.yaml[/]

[bold yellow]HISTORY EXAMPLES:[/]
  List recent scripts:
    [cyan]psw history list[/]

  Show details of a specific entry:
    [cyan]psw history show 3[/]

  Re-run a previous script:
    [cyan]psw history rerun 1[/]

  View statistics:
    [cyan]psw history stats[/]

  Clear all history:
    [cyan]psw history clear[/]

[bold yellow]CACHE EXAMPLES:[/]
  Clear the cache:
    [cyan]psw --clear-cache[/]

  Update package list from marketplace:
    [cyan]psw --update-packages[/]

  Generate script without using cache:
    [cyan]psw --default --no-cache[/]

  Clear cache and generate fresh script:
    [cyan]psw --clear-cache --packages ""uSync"" --project-name MyProject[/]

[bold yellow]VERSIONS EXAMPLES:[/]
  Display Umbraco versions table:
    [cyan]psw versions[/]")
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
  [green]show[/] <name>          Show template details
  [green]delete[/] <name>        Delete a template
  [green]export[/] <name>        Export template to file
  [green]import[/] <file>        Import template from file
  [green]validate[/] <file>      Validate template file

[bold yellow]TEMPLATE OPTIONS:[/]
  [green]    --template-description[/] <desc> Template description
  [green]    --template-tags[/] <tags>   Comma-separated tags
  [green]    --template-file[/] <path>   Template file path
  [green]    --set[/] <key=value>        Override template values

[bold yellow]EXAMPLES:[/]
  Save current configuration as template:
    [cyan]psw template save my-blog --template-description ""My blog setup"" --template-tags ""blog,umbraco14""[/]

  List all templates:
    [cyan]psw template list[/]

  Load and use a template:
    [cyan]psw template load my-blog[/]

  Load template with overrides:
    [cyan]psw template load my-blog --project-name NewBlog --set AutoRun=true[/]

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
  [green]show[/] <#>              Show details of a history entry
  [green]rerun[/] <#>             Regenerate and re-run a script from history
  [green]delete[/] <#>            Delete a history entry
  [green]clear[/]                 Clear all history
  [green]stats[/]                 Show history statistics

[bold yellow]HISTORY OPTIONS:[/]
  [green]    --history-limit[/] <count>  Number of entries to show (default: 10)

[bold yellow]EXAMPLES:[/]
  List recent scripts:
    [cyan]psw history list[/]

  Show details of a specific entry:
    [cyan]psw history show 3[/]

  Re-run a previous script:
    [cyan]psw history rerun 1[/]

  View statistics:
    [cyan]psw history stats[/]

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
