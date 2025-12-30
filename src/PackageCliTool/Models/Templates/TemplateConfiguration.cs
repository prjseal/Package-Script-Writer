namespace PackageCliTool.Models.Templates;

/// <summary>
/// Complete template configuration
/// </summary>
public class TemplateConfiguration
{
    /// <summary>
    /// Umbraco template settings
    /// </summary>
    public TemplateInfo Template { get; set; } = new();

    /// <summary>
    /// Project settings
    /// </summary>
    public ProjectInfo Project { get; set; } = new();

    /// <summary>
    /// List of packages to install
    /// </summary>
    public List<PackageConfig> Packages { get; set; } = new();

    /// <summary>
    /// Starter kit configuration
    /// </summary>
    public StarterKitConfig StarterKit { get; set; } = new();

    /// <summary>
    /// Docker configuration
    /// </summary>
    public DockerConfig Docker { get; set; } = new();

    /// <summary>
    /// Unattended install configuration
    /// </summary>
    public UnattendedConfig Unattended { get; set; } = new();

    /// <summary>
    /// Output format options
    /// </summary>
    public OutputConfig Output { get; set; } = new();

    /// <summary>
    /// Execution options
    /// </summary>
    public ExecutionConfig Execution { get; set; } = new();
}

/// <summary>
/// Umbraco template information
/// </summary>
public class TemplateInfo
{
    /// <summary>
    /// Template name (e.g., "Umbraco.Templates")
    /// </summary>
    public string Name { get; set; } = "Umbraco.Templates";

    /// <summary>
    /// Template version (e.g., "14.3.0", "latest", "lts")
    /// </summary>
    public string Version { get; set; } = "";
}

/// <summary>
/// Project information
/// </summary>
public class ProjectInfo
{
    /// <summary>
    /// Project name
    /// </summary>
    public string Name { get; set; } = "MyProject";

    /// <summary>
    /// Whether to create a solution file
    /// </summary>
    public bool CreateSolution { get; set; } = true;

    /// <summary>
    /// Solution name (if creating solution)
    /// </summary>
    public string? SolutionName { get; set; }
}

/// <summary>
/// Package configuration
/// </summary>
public class PackageConfig
{
    /// <summary>
    /// Package name
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Package version (e.g., "17.0.0", "latest", "prerelease")
    /// </summary>
    public string Version { get; set; } = "latest";
}

/// <summary>
/// Starter kit configuration
/// </summary>
public class StarterKitConfig
{
    /// <summary>
    /// Whether to include a starter kit
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Starter kit package name
    /// </summary>
    public string? Package { get; set; }
}

/// <summary>
/// Docker configuration
/// </summary>
public class DockerConfig
{
    /// <summary>
    /// Include Dockerfile
    /// </summary>
    public bool Dockerfile { get; set; }

    /// <summary>
    /// Include Docker Compose file
    /// </summary>
    public bool DockerCompose { get; set; }

    /// <summary>
    /// Enable Content Delivery API
    /// </summary>
    public bool EnableContentDeliveryApi { get; set; }
}

/// <summary>
/// Unattended install configuration
/// </summary>
public class UnattendedConfig
{
    /// <summary>
    /// Whether to use unattended install
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Database configuration
    /// </summary>
    public DatabaseConfig Database { get; set; } = new();

    /// <summary>
    /// Admin user configuration
    /// </summary>
    public AdminConfig Admin { get; set; } = new();
}

/// <summary>
/// Database configuration
/// </summary>
public class DatabaseConfig
{
    /// <summary>
    /// Database type (SQLite, LocalDb, SQLServer, SQLAzure, SQLCE)
    /// </summary>
    public string Type { get; set; } = "SQLite";

    /// <summary>
    /// Connection string (for SQL Server/Azure)
    /// </summary>
    public string? ConnectionString { get; set; }
}

/// <summary>
/// Admin user configuration
/// </summary>
public class AdminConfig
{
    /// <summary>
    /// Admin user friendly name
    /// </summary>
    public string Name { get; set; } = "Administrator";

    /// <summary>
    /// Admin email address
    /// </summary>
    public string Email { get; set; } = "admin@example.com";

    /// <summary>
    /// Admin password (can be literal, environment variable, or prompt marker)
    /// </summary>
    public string Password { get; set; } = "<prompt>";
}

/// <summary>
/// Output format configuration
/// </summary>
public class OutputConfig
{
    /// <summary>
    /// Output as one-liner
    /// </summary>
    public bool Oneliner { get; set; }

    /// <summary>
    /// Remove comments from script
    /// </summary>
    public bool RemoveComments { get; set; }

    /// <summary>
    /// Include prerelease packages
    /// </summary>
    public bool IncludePrerelease { get; set; }
}

/// <summary>
/// Execution configuration
/// </summary>
public class ExecutionConfig
{
    /// <summary>
    /// Automatically run the script after generation
    /// </summary>
    public bool AutoRun { get; set; }

    /// <summary>
    /// Directory to run the script in
    /// </summary>
    public string RunDirectory { get; set; } = ".";
}
