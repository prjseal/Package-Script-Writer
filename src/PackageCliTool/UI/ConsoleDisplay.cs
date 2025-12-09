using PackageCliTool.Configuration;
using Spectre.Console;
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

[bold yellow]OPTIONS:[/]
  [green]-h, --help[/]                    Show help information
  [green]-v, --version[/]                 Show version information
  [green]-d, --default[/]                 Generate a default script with minimal configuration

[bold yellow]TEMPLATE COMMANDS:[/]
  [green]template save[/] <name>          Save current configuration as a template
  [green]template load[/] <name>          Load and execute a template
  [green]template list[/]                 List all available templates
  [green]template show[/] <name>          Show template details
  [green]template delete[/] <name>        Delete a template
  [green]template export[/] <name>        Export template to file
  [green]template import[/] <file>        Import template from file
  [green]template validate[/] <file>      Validate template file

[bold yellow]TEMPLATE OPTIONS:[/]
  [green]    --template-description[/] <desc> Template description
  [green]    --template-tags[/] <tags>   Comma-separated tags
  [green]    --template-file[/] <path>   Template file path
  [green]    --set[/] <key=value>        Override template values

[bold yellow]HISTORY COMMANDS:[/]
  [green]history list[/]                  List recent script generation history
  [green]history show[/] <#>              Show details of a history entry
  [green]history rerun[/] <#>             Regenerate and re-run a script from history
  [green]history delete[/] <#>            Delete a history entry
  [green]history clear[/]                 Clear all history
  [green]history stats[/]                 Show history statistics

[bold yellow]HISTORY OPTIONS:[/]
  [green]    --history-limit[/] <count>  Number of entries to show (default: 10)

[bold yellow]SCRIPT CONFIGURATION:[/]
  [green]-p, --packages[/] <packages>     Comma-separated list of packages with optional versions
                                   Format: ""Package1|Version1,Package2|Version2""
                                   Or just package names: ""uSync,Umbraco.Forms"" (uses latest)
                                   Example: ""uSync|17.0.0,clean|7.0.1""
  [green]-t, --template-version[/] <ver>  Template version (Latest, LTS, or specific version)
  [green]-n, --project-name[/] <name>     Project name (default: MyProject)
  [green]-s, --solution[/]                Create a solution file
  [green]    --solution-name[/] <name>    Solution name (used with --solution)

[bold yellow]STARTER KIT:[/]
  [green]-k, --starter-kit[/]             Include a starter kit
  [green]    --starter-kit-package[/] <pkg> Starter kit package name

[bold yellow]DOCKER:[/]
  [green]    --dockerfile[/]              Include Dockerfile
  [green]    --docker-compose[/]          Include Docker Compose file

[bold yellow]UNATTENDED INSTALL:[/]
  [green]-u, --unattended[/]              Use unattended install
  [green]    --database-type[/] <type>    Database type (SQLite, LocalDb, SQLServer, SQLAzure, SQLCE)
  [green]    --connection-string[/] <str> Connection string (for SQLServer/SQLAzure)
  [green]    --admin-name[/] <name>       Admin user friendly name
  [green]    --admin-email[/] <email>     Admin email
  [green]    --admin-password[/] <pwd>    Admin password

[bold yellow]OUTPUT OPTIONS:[/]
  [green]-o, --oneliner[/]                Output as one-liner
  [green]-r, --remove-comments[/]         Remove comments from script
  [green]    --include-prerelease[/]      Include prerelease package versions

[bold yellow]CACHE OPTIONS:[/]
  [green]    --no-cache[/]                Disable API response caching (bypass cache)
  [green]    --clear-cache[/]             Clear all cached API responses
  [green]    --update-packages[/]         Update package list cache from marketplace

[bold yellow]EXECUTION:[/]
  [green]    --auto-run[/]                Automatically run the generated script
  [green]    --run-dir[/] <directory>     Directory to run script in

[bold yellow]EXAMPLES:[/]
  Generate default script:
    [cyan]psw --default[/]

  Generate custom script with packages (latest versions):
    [cyan]psw --packages ""uSync,Umbraco.Forms"" --project-name MyProject[/]

  Generate script with specific package versions:
    [cyan]psw --packages ""uSync|17.0.0,clean|7.0.1"" --project-name MyProject[/]

  Mixed: some with versions, some without:
    [cyan]psw -p ""uSync|17.0.0,Umbraco.Forms"" -n MyProject[/]

  Full configuration example:
    [cyan]psw -p ""uSync|17.0.0"" -n MyProject -s --solution-name MySolution -u --database-type SQLite --admin-email admin@test.com --admin-password MyPass123! --auto-run[/]

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
    [cyan]psw --clear-cache --packages ""uSync"" --project-name MyProject[/]")
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

        AnsiConsole.MarkupLine($"[bold]Version:[/] {ApiConfiguration.Version}");
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


}
