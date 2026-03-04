using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace PSW.Controllers;

/// <summary>
/// API controller for serving community templates
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CommunityTemplatesApiController : ControllerBase
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CommunityTemplatesApiController> _logger;
    private const int CacheTimeInMinutes = 60;

    public CommunityTemplatesApiController(
        IWebHostEnvironment webHostEnvironment,
        IMemoryCache memoryCache,
        ILogger<CommunityTemplatesApiController> logger)
    {
        _webHostEnvironment = webHostEnvironment;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    /// <summary>
    /// Gets the community templates index containing metadata for all available templates
    /// </summary>
    /// <returns>The templates index as JSON</returns>
    /// <response code="200">Returns the templates index</response>
    /// <response code="404">If the index file is not found</response>
    /// <response code="500">If there's an error reading the index file</response>
    [HttpGet("index")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetIndex()
    {
        try
        {
            var cacheKey = "community_templates_index";

            var indexContent = await _memoryCache.GetOrCreateAsync(
                cacheKey,
                async cacheEntry =>
                {
                    cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheTimeInMinutes);

                    var indexPath = Path.Combine(_webHostEnvironment.WebRootPath,
                        "community-templates",
                        "index.json");

                    if (!System.IO.File.Exists(indexPath))
                    {
                        _logger.LogWarning("Community templates index not found at {IndexPath}", indexPath);
                        return null;
                    }

                    _logger.LogInformation("Reading community templates index from {IndexPath}", indexPath);
                    return await System.IO.File.ReadAllTextAsync(indexPath);
                });

            if (indexContent == null)
            {
                return NotFound(new { error = "Community templates index not found" });
            }

            return Content(indexContent, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading community templates index");
            return StatusCode(500, new { error = "Error reading community templates index", message = ex.Message });
        }
    }

    /// <summary>
    /// Gets a specific community template by filename
    /// </summary>
    /// <param name="fileName">The template filename (e.g., 'uskinned-shop.yaml')</param>
    /// <returns>The template content as plain text (YAML format)</returns>
    /// <response code="200">Returns the template content</response>
    /// <response code="400">If the filename is invalid</response>
    /// <response code="404">If the template file is not found</response>
    /// <response code="500">If there's an error reading the template file</response>
    [HttpGet("template/{fileName}")]
    public async Task<IActionResult> GetTemplate(string fileName)
    {
        // Sanitize user input immediately for logging - only allow letters and spaces
        var templateName = Regex.Replace(fileName ?? string.Empty, @"[^a-zA-Z ]", string.Empty);

        try
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest(new { error = "Filename is required" });
            }

            // Security: Ensure the filename doesn't contain path traversal attempts
            if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
            {
                _logger.LogWarning("Invalid filename attempted: {TemplateName}", templateName);
                return BadRequest(new { error = "Invalid filename" });
            }

            // Ensure the file has a .yaml extension
            if (!fileName.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".yaml";
            }

            var cacheKey = $"community_template_{fileName}";

            var templateContent = await _memoryCache.GetOrCreateAsync(
                cacheKey,
                async cacheEntry =>
                {
                    cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheTimeInMinutes);

                    // CHANGED: Use wwwroot/community-templates instead of content root
                    var templatePath = Path.Combine(
                        _webHostEnvironment.WebRootPath, // Use WebRootPath for wwwroot
                        "community-templates",
                        fileName);

                    if (!System.IO.File.Exists(templatePath))
                    {
                        _logger.LogWarning("Community template not found: {TemplateName}", templateName);
                        return null;
                    }

                    _logger.LogInformation("Reading community template: {TemplateName}", templateName);
                    return await System.IO.File.ReadAllTextAsync(templatePath);
                });

            if (templateContent == null)
            {
                return NotFound(new { error = $"Template '{templateName}' not found" });
            }

            return Content(templateContent, "text/plain");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading community template {TemplateName}", templateName);
            return StatusCode(500, new { error = "Error reading community template", message = ex.Message });
        }
    }

    /// <summary>
    /// Clears the cache for community templates
    /// </summary>
    /// <returns>A confirmation message with timestamp</returns>
    /// <response code="200">Returns a confirmation message</response>
    [HttpGet("clearcache")]
    [Produces("text/plain")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult ClearCache()
    {
        try
        {
            // Clear index cache
            _memoryCache.Remove("community_templates_index");

            // Note: We can't easily enumerate all cached templates, but they will expire naturally
            _logger.LogInformation("Community templates cache cleared");

            var message = $"Community templates cache cleared at {DateTime.Now}";
            return Ok(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing community templates cache");
            return StatusCode(500, new { error = "Error clearing cache", message = ex.Message });
        }
    }
}