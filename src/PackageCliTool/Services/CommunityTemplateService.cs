using System.Text.Json;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using PackageCliTool.Models.CommunityTemplates;
using PackageCliTool.Models.Templates;
using PackageCliTool.Exceptions;

namespace PackageCliTool.Services;

/// <summary>
/// Service for fetching and managing community templates from GitHub
/// </summary>
public class CommunityTemplateService
{
    private const string DefaultRepo = "prjseal/Package-Script-Writer";
    private const string DefaultBranch = "main";
    private const string CommunityTemplatesCacheKey = "community_templates_index";

    private readonly HttpClient _httpClient;
    private readonly ILogger? _logger;
    private readonly CacheService? _cacheService;
    private readonly IDeserializer _deserializer;
    private readonly string _repository;
    private readonly string _branch;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommunityTemplateService"/> class
    /// </summary>
    /// <param name="httpClient">HTTP client for making requests</param>
    /// <param name="repository">GitHub repository (owner/repo format)</param>
    /// <param name="branch">Git branch to fetch from</param>
    /// <param name="cacheService">Optional cache service</param>
    /// <param name="logger">Optional logger instance</param>
    public CommunityTemplateService(
        HttpClient? httpClient = null,
        string? repository = null,
        string? branch = null,
        CacheService? cacheService = null,
        ILogger? logger = null)
    {
        _httpClient = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        _repository = repository ?? DefaultRepo;
        _branch = branch ?? DefaultBranch;
        _cacheService = cacheService;
        _logger = logger;

        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    /// <summary>
    /// Gets the URL for the template index
    /// </summary>
    private string GetIndexUrl()
        => $"https://raw.githubusercontent.com/{_repository}/{_branch}/community-templates/index.json";

    /// <summary>
    /// Gets the URL for a specific template file
    /// </summary>
    private string GetTemplateUrl(string fileName)
        => $"https://raw.githubusercontent.com/{_repository}/{_branch}/community-templates/{fileName}";

    /// <summary>
    /// Fetches the template index from GitHub
    /// </summary>
    public async Task<TemplateIndex> GetIndexAsync()
    {
        try
        {
            // Check cache first
            var cachedIndex = _cacheService?.Get(CommunityTemplatesCacheKey, CacheType.Package);
            if (cachedIndex != null)
            {
                _logger?.LogDebug("Using cached community templates index");
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var index = JsonSerializer.Deserialize<TemplateIndex>(cachedIndex, options);
                    if (index != null)
                    {
                        return index;
                    }
                }
                catch (JsonException ex)
                {
                    _logger?.LogWarning(ex, "Failed to deserialize cached index, fetching fresh data");
                }
            }

            // Fetch from GitHub
            var url = GetIndexUrl();
            _logger?.LogInformation("Fetching community templates index from {Url}", url);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            TemplateIndex? templateIndex;
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                templateIndex = JsonSerializer.Deserialize<TemplateIndex>(content, options);
            }
            catch (JsonException ex)
            {
                _logger?.LogError(ex, "Failed to parse community templates index");
                throw new PswException(
                    "PSW-CT-001",
                    "Failed to parse community templates index",
                    "Failed to parse community templates index",
                    "The index.json file may be malformed"
                );
            }

            if (templateIndex == null)
            {
                throw new PswException(
                    "PSW-CT-002",
                    "Failed to parse community templates index",
                    "Failed to parse community templates index",
                    "The index.json file may be malformed"
                );
            }

            // Cache the index
            _cacheService?.Set(CommunityTemplatesCacheKey, content, CacheType.Package, ttlHours: 1);

            _logger?.LogInformation("Loaded {Count} community templates", templateIndex.Templates.Count);
            return templateIndex;
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "Failed to fetch community templates index");
            throw new PswException(
                "PSW-CT-003",
                "Unable to fetch community templates",
                "Unable to fetch community templates",
                "Check your internet connection or try again later"
            );
        }
    }

    /// <summary>
    /// Gets all available community templates
    /// </summary>
    public async Task<List<CommunityTemplateMetadata>> GetAllTemplatesAsync()
    {
        var index = await GetIndexAsync();
        return index.Templates;
    }

    /// <summary>
    /// Fetches a specific community template by name
    /// </summary>
    /// <param name="templateName">The template name (not the file name)</param>
    public async Task<Template> GetTemplateAsync(string templateName)
    {
        try
        {
            // Get index to find the template
            var index = await GetIndexAsync();
            var metadata = index.Templates.FirstOrDefault(t =>
                t.Name.Equals(templateName, StringComparison.OrdinalIgnoreCase));

            if (metadata == null)
            {
                var availableTemplates = string.Join(", ", index.Templates.Select(t => t.Name));
                throw new PswException(
                    "PSW-CT-004",
                    $"Community template '{templateName}' not found",
                    $"Community template '{templateName}' not found",
                    $"Available templates: {availableTemplates}"
                );
            }

            // Fetch the template YAML
            var url = GetTemplateUrl(metadata.FileName);
            _logger?.LogInformation("Fetching community template from {Url}", url);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var yamlContent = await response.Content.ReadAsStringAsync();

            // Deserialize the template
            Template? template;
            try
            {
                template = _deserializer.Deserialize<Template>(yamlContent);
            }
            catch (Exception ex) when (ex is YamlDotNet.Core.YamlException || ex is ArgumentException)
            {
                _logger?.LogError(ex, "Failed to parse community template '{TemplateName}'", templateName);
                throw new PswException(
                    "PSW-CT-005",
                    $"Failed to parse community template '{templateName}'",
                    $"Failed to parse community template '{templateName}'",
                    "The template YAML file may be malformed"
                );
            }

            if (template == null)
            {
                throw new PswException(
                    "PSW-CT-006",
                    $"Failed to parse community template '{templateName}'",
                    $"Failed to parse community template '{templateName}'",
                    "The template YAML file may be malformed"
                );
            }

            _logger?.LogInformation("Loaded community template: {Name}", template.Metadata.Name);
            return template;
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "Failed to fetch community template '{TemplateName}'", templateName);
            throw new PswException(
                "PSW-CT-007",
                $"Unable to fetch community template '{templateName}'",
                $"Unable to fetch community template '{templateName}'",
                "Check your internet connection or try again later"
            );
        }
        catch (PswException)
        {
            // Re-throw PswException from template not found or parsing errors
            throw;
        }
    }

    /// <summary>
    /// Gets the raw YAML content of a community template
    /// </summary>
    public async Task<string> GetTemplateContentAsync(string templateName)
    {
        var index = await GetIndexAsync();
        var metadata = index.Templates.FirstOrDefault(t =>
            t.Name.Equals(templateName, StringComparison.OrdinalIgnoreCase));

        if (metadata == null)
        {
            throw new PswException(
                "PSW-CT-008",
                $"Community template '{templateName}' not found",
                $"Community template '{templateName}' not found",
                "Use --community-template list to see available templates"
            );
        }

        var url = GetTemplateUrl(metadata.FileName);
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// Clears the cached community templates index
    /// </summary>
    public void ClearCache()
    {
        _cacheService?.Clear(CacheType.Package);
        _logger?.LogInformation("Cleared community templates cache");
    }
}
