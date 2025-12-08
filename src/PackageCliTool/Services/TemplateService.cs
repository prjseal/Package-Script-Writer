using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using PackageCliTool.Models.Templates;
using PackageCliTool.Models.Api;
using PackageCliTool.Exceptions;

namespace PackageCliTool.Services;

/// <summary>
/// Service for managing templates (CRUD operations)
/// </summary>
public class TemplateService
{
    private readonly string _templatesDirectory;
    private readonly ILogger? _logger;
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;

    public TemplateService(string? templatesDirectory = null, ILogger? logger = null)
    {
        // Default to ~/.psw/templates/
        _templatesDirectory = templatesDirectory ?? GetDefaultTemplatesDirectory();
        _logger = logger;

        // Initialize YAML serializer/deserializer
        _serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        EnsureDirectoryExists();
    }

    /// <summary>
    /// Gets the default templates directory path
    /// </summary>
    private static string GetDefaultTemplatesDirectory()
    {
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(homeDir, ".psw", "templates");
    }

    /// <summary>
    /// Ensures the templates directory exists
    /// </summary>
    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(_templatesDirectory))
        {
            Directory.CreateDirectory(_templatesDirectory);
            _logger?.LogInformation("Created templates directory: {Directory}", _templatesDirectory);
        }
    }

    /// <summary>
    /// Saves a template to disk
    /// </summary>
    public async Task SaveTemplateAsync(Template template)
    {
        if (string.IsNullOrWhiteSpace(template.Metadata.Name))
        {
            throw new PswException("Template name cannot be empty", "Provide a valid template name");
        }

        // Update modified timestamp
        template.Metadata.Modified = DateTime.UtcNow;

        var filePath = GetTemplatePath(template.Metadata.Name);
        var yaml = _serializer.Serialize(template);

        await File.WriteAllTextAsync(filePath, yaml);
        _logger?.LogInformation("Template saved: {Name} at {Path}", template.Metadata.Name, filePath);
    }

    /// <summary>
    /// Loads a template from disk
    /// </summary>
    public async Task<Template> LoadTemplateAsync(string name)
    {
        var filePath = GetTemplatePath(name);

        if (!File.Exists(filePath))
        {
            throw new PswException(
                $"Template '{name}' not found",
                $"Available templates: {string.Join(", ", GetTemplateNames())}"
            );
        }

        _logger?.LogInformation("Loading template: {Name} from {Path}", name, filePath);

        var yaml = await File.ReadAllTextAsync(filePath);
        var template = _deserializer.Deserialize<Template>(yaml);

        return template;
    }

    /// <summary>
    /// Lists all available templates
    /// </summary>
    public List<TemplateMetadata> ListTemplates(string? tagFilter = null)
    {
        var templates = new List<TemplateMetadata>();

        if (!Directory.Exists(_templatesDirectory))
        {
            return templates;
        }

        foreach (var file in Directory.GetFiles(_templatesDirectory, "*.yaml"))
        {
            try
            {
                var yaml = File.ReadAllText(file);
                var template = _deserializer.Deserialize<Template>(yaml);

                // Apply tag filter if specified
                if (tagFilter == null || template.Metadata.Tags.Contains(tagFilter, StringComparer.OrdinalIgnoreCase))
                {
                    templates.Add(template.Metadata);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to load template from {File}", file);
            }
        }

        return templates.OrderBy(t => t.Name).ToList();
    }

    /// <summary>
    /// Checks if a template exists
    /// </summary>
    public bool TemplateExists(string name)
    {
        var filePath = GetTemplatePath(name);
        return File.Exists(filePath);
    }

    /// <summary>
    /// Deletes a template
    /// </summary>
    public void DeleteTemplate(string name)
    {
        var filePath = GetTemplatePath(name);

        if (!File.Exists(filePath))
        {
            throw new PswException(
                $"Template '{name}' not found",
                $"Available templates: {string.Join(", ", GetTemplateNames())}"
            );
        }

        File.Delete(filePath);
        _logger?.LogInformation("Template deleted: {Name}", name);
    }

    /// <summary>
    /// Exports a template to a specific file path
    /// </summary>
    public async Task ExportTemplateAsync(string name, string outputPath)
    {
        var template = await LoadTemplateAsync(name);
        var yaml = _serializer.Serialize(template);

        await File.WriteAllTextAsync(outputPath, yaml);
        _logger?.LogInformation("Template exported: {Name} to {Path}", name, outputPath);
    }

    /// <summary>
    /// Imports a template from a file
    /// </summary>
    public async Task<Template> ImportTemplateAsync(string filePath, string? newName = null)
    {
        if (!File.Exists(filePath))
        {
            throw new PswException(
                $"Template file not found: {filePath}",
                "Provide a valid template file path"
            );
        }

        var yaml = await File.ReadAllTextAsync(filePath);
        var template = _deserializer.Deserialize<Template>(yaml);

        // Optionally rename the template
        if (!string.IsNullOrWhiteSpace(newName))
        {
            template.Metadata.Name = newName;
        }

        // Save to templates directory
        await SaveTemplateAsync(template);
        _logger?.LogInformation("Template imported: {Name} from {Path}", template.Metadata.Name, filePath);

        return template;
    }

    /// <summary>
    /// Converts a Template to a ScriptModel for API generation
    /// </summary>
    public ScriptModel ToScriptModel(Template template, Dictionary<string, object>? overrides = null)
    {
        var config = template.Configuration;

        // Apply overrides if provided
        if (overrides != null)
        {
            config = ApplyOverrides(config, overrides);
        }

        var model = new ScriptModel
        {
            TemplateName = config.Template.Name,
            TemplateVersion = config.Template.Version,
            ProjectName = config.Project.Name,
            CreateSolutionFile = config.Project.CreateSolution,
            SolutionName = config.Project.SolutionName,
            Packages = BuildPackagesString(config.Packages),
            IncludeStarterKit = config.StarterKit.Enabled,
            StarterKitPackage = config.StarterKit.Package,
            IncludeDockerfile = config.Docker.Dockerfile,
            IncludeDockerCompose = config.Docker.DockerCompose,
            CanIncludeDocker = config.Docker.Dockerfile || config.Docker.DockerCompose,
            UseUnattendedInstall = config.Unattended.Enabled,
            DatabaseType = config.Unattended.Database.Type,
            ConnectionString = config.Unattended.Database.ConnectionString,
            UserFriendlyName = config.Unattended.Admin.Name,
            UserEmail = config.Unattended.Admin.Email,
            UserPassword = ResolvePassword(config.Unattended.Admin.Password),
            OnelinerOutput = config.Output.Oneliner,
            RemoveComments = config.Output.RemoveComments
        };

        return model;
    }

    /// <summary>
    /// Creates a Template from command-line options
    /// </summary>
    public Template FromCommandLineOptions(Models.CommandLineOptions options, string templateName, string? description = null)
    {
        var template = new Template
        {
            Metadata = new TemplateMetadata
            {
                Name = templateName,
                Description = description ?? $"Template created from command-line options",
                Author = Environment.UserName,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow,
                Version = "1.0.0"
            },
            Configuration = new TemplateConfiguration
            {
                Template = new TemplateInfo
                {
                    Name = "Umbraco.Templates",
                    Version = options.TemplateVersion ?? ""
                },
                Project = new ProjectInfo
                {
                    Name = options.ProjectName ?? "MyProject",
                    CreateSolution = options.CreateSolution,
                    SolutionName = options.SolutionName
                },
                Packages = ParsePackages(options.Packages),
                StarterKit = new StarterKitConfig
                {
                    Enabled = options.IncludeStarterKit,
                    Package = options.StarterKitPackage
                },
                Docker = new DockerConfig
                {
                    Dockerfile = options.IncludeDockerfile,
                    DockerCompose = options.IncludeDockerCompose
                },
                Unattended = new UnattendedConfig
                {
                    Enabled = options.UseUnattended,
                    Database = new DatabaseConfig
                    {
                        Type = options.DatabaseType ?? "SQLite",
                        ConnectionString = options.ConnectionString
                    },
                    Admin = new AdminConfig
                    {
                        Name = options.AdminName ?? "Administrator",
                        Email = options.AdminEmail ?? "admin@example.com",
                        Password = options.AdminPassword ?? "<prompt>"
                    }
                },
                Output = new OutputConfig
                {
                    Oneliner = options.OnelinerOutput,
                    RemoveComments = options.RemoveComments,
                    IncludePrerelease = options.IncludePrerelease
                },
                Execution = new ExecutionConfig
                {
                    AutoRun = options.AutoRun,
                    RunDirectory = options.RunDirectory ?? "."
                }
            }
        };

        return template;
    }

    /// <summary>
    /// Creates a Template from a ScriptModel and package versions
    /// </summary>
    public Template FromScriptModel(ScriptModel scriptModel, Dictionary<string, string> packageVersions, string templateName, string? description = null)
    {
        var template = new Template
        {
            Metadata = new TemplateMetadata
            {
                Name = templateName,
                Description = description ?? $"Template created from interactive script generation",
                Author = Environment.UserName,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow,
                Version = "1.0.0"
            },
            Configuration = new TemplateConfiguration
            {
                Template = new TemplateInfo
                {
                    Name = scriptModel.TemplateName,
                    Version = scriptModel.TemplateVersion ?? ""
                },
                Project = new ProjectInfo
                {
                    Name = scriptModel.ProjectName,
                    CreateSolution = scriptModel.CreateSolutionFile,
                    SolutionName = scriptModel.SolutionName
                },
                Packages = ConvertPackageVersionsToConfigs(packageVersions),
                StarterKit = new StarterKitConfig
                {
                    Enabled = scriptModel.IncludeStarterKit,
                    Package = scriptModel.StarterKitPackage
                },
                Docker = new DockerConfig
                {
                    Dockerfile = scriptModel.IncludeDockerfile,
                    DockerCompose = scriptModel.IncludeDockerCompose
                },
                Unattended = new UnattendedConfig
                {
                    Enabled = scriptModel.UseUnattendedInstall,
                    Database = new DatabaseConfig
                    {
                        Type = scriptModel.DatabaseType ?? "SQLite",
                        ConnectionString = scriptModel.ConnectionString
                    },
                    Admin = new AdminConfig
                    {
                        Name = scriptModel.UserFriendlyName ?? "Administrator",
                        Email = scriptModel.UserEmail ?? "admin@example.com",
                        Password = "<prompt>" // Don't save actual password
                    }
                },
                Output = new OutputConfig
                {
                    Oneliner = scriptModel.OnelinerOutput,
                    RemoveComments = scriptModel.RemoveComments,
                    IncludePrerelease = false
                },
                Execution = new ExecutionConfig
                {
                    AutoRun = false,
                    RunDirectory = "."
                }
            }
        };

        return template;
    }

    /// <summary>
    /// Converts packageVersions dictionary to PackageConfig list
    /// </summary>
    private List<PackageConfig> ConvertPackageVersionsToConfigs(Dictionary<string, string> packageVersions)
    {
        var packages = new List<PackageConfig>();

        foreach (var (packageName, version) in packageVersions)
        {
            string versionValue;

            if (string.IsNullOrEmpty(version))
            {
                versionValue = "latest";
            }
            else if (version == "--prerelease")
            {
                versionValue = "prerelease";
            }
            else
            {
                versionValue = version;
            }

            packages.Add(new PackageConfig
            {
                Name = packageName,
                Version = versionValue
            });
        }

        return packages;
    }

    /// <summary>
    /// Gets all template names
    /// </summary>
    private List<string> GetTemplateNames()
    {
        if (!Directory.Exists(_templatesDirectory))
        {
            return new List<string>();
        }

        return Directory.GetFiles(_templatesDirectory, "*.yaml")
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .ToList();
    }

    /// <summary>
    /// Gets the full path for a template file
    /// </summary>
    private string GetTemplatePath(string name)
    {
        return Path.Combine(_templatesDirectory, $"{name}.yaml");
    }

    /// <summary>
    /// Builds a packages string from package configurations
    /// </summary>
    private string BuildPackagesString(List<PackageConfig> packages)
    {
        if (packages.Count == 0)
        {
            return "";
        }

        var parts = new List<string>();

        foreach (var pkg in packages)
        {
            if (string.IsNullOrWhiteSpace(pkg.Version) || pkg.Version.Equals("latest", StringComparison.OrdinalIgnoreCase))
            {
                // Latest version - just package name
                parts.Add(pkg.Name);
            }
            else if (pkg.Version.Equals("prerelease", StringComparison.OrdinalIgnoreCase))
            {
                // Prerelease - package name with flag
                parts.Add($"{pkg.Name} --prerelease");
            }
            else
            {
                // Specific version - package|version format
                parts.Add($"{pkg.Name}|{pkg.Version}");
            }
        }

        return string.Join(",", parts);
    }

    /// <summary>
    /// Parses packages string into PackageConfig list
    /// </summary>
    private List<PackageConfig> ParsePackages(string? packagesString)
    {
        if (string.IsNullOrWhiteSpace(packagesString))
        {
            return new List<PackageConfig>();
        }

        var packages = new List<PackageConfig>();
        var entries = packagesString.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (var entry in entries)
        {
            var trimmed = entry.Trim();

            if (trimmed.Contains('|'))
            {
                // Format: "PackageName|Version"
                var parts = trimmed.Split('|', 2);
                packages.Add(new PackageConfig
                {
                    Name = parts[0].Trim(),
                    Version = parts[1].Trim()
                });
            }
            else if (trimmed.Contains("--prerelease"))
            {
                // Format: "PackageName --prerelease"
                var packageName = trimmed.Replace("--prerelease", "").Trim();
                packages.Add(new PackageConfig
                {
                    Name = packageName,
                    Version = "prerelease"
                });
            }
            else
            {
                // Format: "PackageName" (latest)
                packages.Add(new PackageConfig
                {
                    Name = trimmed,
                    Version = "latest"
                });
            }
        }

        return packages;
    }

    /// <summary>
    /// Resolves password from various formats (literal, env var, prompt)
    /// </summary>
    private string ResolvePassword(string passwordSpec)
    {
        // Check for prompt marker
        if (passwordSpec == "<prompt>")
        {
            // Will be prompted at runtime
            return "";
        }

        // Check for environment variable: ${VAR_NAME}
        if (passwordSpec.StartsWith("${") && passwordSpec.EndsWith("}"))
        {
            var varName = passwordSpec[2..^1];
            var value = Environment.GetEnvironmentVariable(varName);

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new PswException(
                    $"Environment variable '{varName}' not found",
                    $"Set the environment variable or update the template"
                );
            }

            return value;
        }

        // Literal password
        return passwordSpec;
    }

    /// <summary>
    /// Applies overrides to configuration (simple implementation)
    /// </summary>
    private TemplateConfiguration ApplyOverrides(TemplateConfiguration config, Dictionary<string, object> overrides)
    {
        // Simple implementation - can be extended for nested property paths
        foreach (var (key, value) in overrides)
        {
            switch (key.ToLower())
            {
                case "projectname":
                    config.Project.Name = value.ToString() ?? config.Project.Name;
                    break;
                case "solutionname":
                    config.Project.SolutionName = value.ToString();
                    break;
                case "databasetype":
                    config.Unattended.Database.Type = value.ToString() ?? config.Unattended.Database.Type;
                    break;
                case "autorun":
                    config.Execution.AutoRun = Convert.ToBoolean(value);
                    break;
                case "rundirectory":
                    config.Execution.RunDirectory = value.ToString() ?? config.Execution.RunDirectory;
                    break;
            }
        }

        return config;
    }
}
