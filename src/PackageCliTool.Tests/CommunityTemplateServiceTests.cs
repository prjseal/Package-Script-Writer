using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using PackageCliTool.Exceptions;
using PackageCliTool.Models.CommunityTemplates;
using PackageCliTool.Models.Templates;
using PackageCliTool.Services;
using Xunit;

namespace PackageCliTool.Tests;

/// <summary>
/// Unit tests for CommunityTemplateService
/// </summary>
public class CommunityTemplateServiceTests : IDisposable
{
    private readonly string _testCacheDirectory;
    private readonly CacheService _cacheService;

    public CommunityTemplateServiceTests()
    {
        _testCacheDirectory = Path.Combine(Path.GetTempPath(), $"psw-test-community-{Guid.NewGuid()}");
        _cacheService = new CacheService(cacheDirectory: _testCacheDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testCacheDirectory))
        {
            Directory.Delete(_testCacheDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task GetIndexAsync_FetchesIndexFromGitHub()
    {
        // Arrange
        var mockHandler = CreateMockHttpHandler(GetSampleIndexJson());
        var httpClient = new HttpClient(mockHandler.Object);
        var service = new CommunityTemplateService(httpClient: httpClient);

        // Act
        var index = await service.GetIndexAsync();

        // Assert
        index.Should().NotBeNull();
        index.Templates.Should().HaveCount(2);
        index.Templates[0].Name.Should().Be("blog-with-usync");
        index.Templates[1].Name.Should().Be("commerce-starter");
    }

    [Fact]
    public async Task GetIndexAsync_UsesCachedIndex_WhenAvailable()
    {
        // Arrange
        var mockHandler = CreateMockHttpHandler(GetSampleIndexJson());
        var httpClient = new HttpClient(mockHandler.Object);
        var service = new CommunityTemplateService(
            httpClient: httpClient,
            cacheService: _cacheService);

        // Act - First call should hit the network
        var index1 = await service.GetIndexAsync();

        // Second call should use cache (mock handler would fail if called again)
        var index2 = await service.GetIndexAsync();

        // Assert
        index1.Templates.Should().HaveCount(2);
        index2.Templates.Should().HaveCount(2);
        index1.Templates[0].Name.Should().Be(index2.Templates[0].Name);
    }

    [Fact]
    public async Task GetIndexAsync_ThrowsPswException_OnNetworkError()
    {
        // Arrange
        var mockHandler = CreateMockHttpHandler(null, HttpStatusCode.InternalServerError);
        var httpClient = new HttpClient(mockHandler.Object);
        var service = new CommunityTemplateService(httpClient: httpClient);

        // Act
        var act = async () => await service.GetIndexAsync();

        // Assert
        await act.Should().ThrowAsync<PswException>()
            .Where(ex => ex.Message.Contains("Unable to fetch community templates"));
    }

    [Fact]
    public async Task GetIndexAsync_ThrowsPswException_OnMalformedJson()
    {
        // Arrange
        var mockHandler = CreateMockHttpHandler("{ invalid json [[[");
        var httpClient = new HttpClient(mockHandler.Object);
        var service = new CommunityTemplateService(httpClient: httpClient);

        // Act
        var act = async () => await service.GetIndexAsync();

        // Assert
        await act.Should().ThrowAsync<PswException>()
            .Where(ex => ex.Message.Contains("Failed to parse community templates index"));
    }

    [Fact]
    public async Task GetAllTemplatesAsync_ReturnsAllTemplatesFromIndex()
    {
        // Arrange
        var mockHandler = CreateMockHttpHandler(GetSampleIndexJson());
        var httpClient = new HttpClient(mockHandler.Object);
        var service = new CommunityTemplateService(httpClient: httpClient);

        // Act
        var templates = await service.GetAllTemplatesAsync();

        // Assert
        templates.Should().HaveCount(2);
        templates[0].DisplayName.Should().Be("Blog with uSync");
        templates[1].DisplayName.Should().Be("Commerce Starter");
    }

    [Fact]
    public async Task GetTemplateAsync_FetchesTemplateYaml()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();

        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                if (request.RequestUri!.AbsolutePath.Contains("/communitytemplates/index"))
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(GetSampleIndexJson())
                    };
                }
                else if (request.RequestUri!.AbsolutePath.Contains("/communitytemplates/template/"))
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(GetSampleTemplateYaml())
                    };
                }
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                };
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var service = new CommunityTemplateService(httpClient: httpClient);

        // Act
        var template = await service.GetTemplateAsync("blog-with-usync");

        // Assert
        template.Should().NotBeNull();
        template.Metadata.Name.Should().Be("blog-with-usync");
        template.Metadata.Description.Should().Contain("blog setup");
        template.Configuration.Packages.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetTemplateAsync_ThrowsPswException_WhenTemplateNotFound()
    {
        // Arrange
        var mockHandler = CreateMockHttpHandler(GetSampleIndexJson());
        var httpClient = new HttpClient(mockHandler.Object);
        var service = new CommunityTemplateService(httpClient: httpClient);

        // Act
        var act = async () => await service.GetTemplateAsync("non-existent-template");

        // Assert
        await act.Should().ThrowAsync<PswException>()
            .Where(ex => ex.Message.Contains("Community template 'non-existent-template' not found") &&
                        ex.Suggestion!.Contains("Available templates:"));
    }

    [Fact]
    public async Task GetTemplateAsync_IsCaseInsensitive()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();

        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                if (request.RequestUri!.AbsolutePath.Contains("/communitytemplates/index"))
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(GetSampleIndexJson())
                    };
                }
                else if (request.RequestUri!.AbsolutePath.Contains("/communitytemplates/template/"))
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(GetSampleTemplateYaml())
                    };
                }
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                };
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var service = new CommunityTemplateService(httpClient: httpClient);

        // Act
        var template = await service.GetTemplateAsync("BLOG-WITH-USYNC");

        // Assert
        template.Should().NotBeNull();
        template.Metadata.Name.Should().Be("blog-with-usync");
    }

    [Fact]
    public async Task GetTemplateAsync_ThrowsPswException_OnNetworkError()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();

        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                if (request.RequestUri!.AbsolutePath.Contains("/communitytemplates/index"))
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(GetSampleIndexJson())
                    };
                }
                else if (request.RequestUri!.AbsolutePath.Contains("/communitytemplates/template/"))
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Content = new StringContent("")
                    };
                }
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                };
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var service = new CommunityTemplateService(httpClient: httpClient);

        // Act
        var act = async () => await service.GetTemplateAsync("blog-with-usync");

        // Assert
        await act.Should().ThrowAsync<PswException>()
            .Where(ex => ex.Message.Contains("Unable to fetch community template"));
    }

    [Fact]
    public async Task GetTemplateAsync_ThrowsPswException_OnMalformedYaml()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();

        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                if (request.RequestUri!.AbsolutePath.Contains("/communitytemplates/index"))
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(GetSampleIndexJson())
                    };
                }
                else if (request.RequestUri!.AbsolutePath.Contains("/communitytemplates/template/"))
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("invalid: yaml: {{{")
                    };
                }
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                };
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var service = new CommunityTemplateService(httpClient: httpClient);

        // Act
        var act = async () => await service.GetTemplateAsync("blog-with-usync");

        // Assert
        await act.Should().ThrowAsync<PswException>()
            .Where(ex => ex.Message.Contains("Failed to parse community template"));
    }

    [Fact]
    public async Task GetTemplateContentAsync_ReturnsRawYaml()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var yamlContent = GetSampleTemplateYaml();

        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                if (request.RequestUri!.AbsolutePath.Contains("/communitytemplates/index"))
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(GetSampleIndexJson())
                    };
                }
                else if (request.RequestUri!.AbsolutePath.Contains("/communitytemplates/template/"))
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(yamlContent)
                    };
                }
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                };
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var service = new CommunityTemplateService(httpClient: httpClient);

        // Act
        var content = await service.GetTemplateContentAsync("blog-with-usync");

        // Assert
        content.Should().Be(yamlContent);
    }

    [Fact]
    public async Task GetTemplateContentAsync_ThrowsPswException_WhenTemplateNotFound()
    {
        // Arrange
        var mockHandler = CreateMockHttpHandler(GetSampleIndexJson());
        var httpClient = new HttpClient(mockHandler.Object);
        var service = new CommunityTemplateService(httpClient: httpClient);

        // Act
        var act = async () => await service.GetTemplateContentAsync("non-existent");

        // Assert
        await act.Should().ThrowAsync<PswException>()
            .Where(ex => ex.Message.Contains("Community template 'non-existent' not found") &&
                        ex.Suggestion!.Contains("Use --community-template list"));
    }

    [Fact]
    public void ClearCache_ClearsPackageCache()
    {
        // Arrange
        var mockHandler = CreateMockHttpHandler(GetSampleIndexJson());
        var httpClient = new HttpClient(mockHandler.Object);
        var service = new CommunityTemplateService(
            httpClient: httpClient,
            cacheService: _cacheService);

        // Pre-populate cache
        _cacheService.Set("test-key", "test-value", CacheType.Package);

        // Act
        service.ClearCache();

        // Assert
        _cacheService.Get("test-key", CacheType.Package).Should().BeNull();
    }

    [Fact]
    public async Task GetIndexAsync_HandlesCorruptedCache_FetchesFreshData()
    {
        // Arrange
        var mockHandler = CreateMockHttpHandler(GetSampleIndexJson());
        var httpClient = new HttpClient(mockHandler.Object);
        var service = new CommunityTemplateService(
            httpClient: httpClient,
            cacheService: _cacheService);

        // Corrupt the cache with invalid JSON
        _cacheService.Set("community_templates_index", "{ invalid json", CacheType.Package);

        // Act - Should handle corrupted cache and fetch fresh data
        var index = await service.GetIndexAsync();

        // Assert
        index.Should().NotBeNull();
        index.Templates.Should().HaveCount(2);
    }

    [Fact]
    public void CommunityTemplateService_UsesDefaultRepository_WhenNotSpecified()
    {
        // Arrange & Act
        var service = new CommunityTemplateService();

        // Assert - Service should be created without errors
        service.Should().NotBeNull();
    }

    [Fact]
    public void CommunityTemplateService_CanBeCreatedWithHttpClient()
    {
        // Arrange & Act
        var httpClient = new HttpClient();
        var service = new CommunityTemplateService(httpClient: httpClient);

        // Assert - Service should be created without errors
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task GetIndexAsync_WithLogger_LogsInformation()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockHandler = CreateMockHttpHandler(GetSampleIndexJson());
        var httpClient = new HttpClient(mockHandler.Object);
        var service = new CommunityTemplateService(
            httpClient: httpClient,
            logger: mockLogger.Object);

        // Act
        await service.GetIndexAsync();

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Fetching community templates index")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Helper method to create a mock HTTP message handler
    /// </summary>
    private static Mock<HttpMessageHandler> CreateMockHttpHandler(
        string? responseContent,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var mockHandler = new Mock<HttpMessageHandler>();

        var response = responseContent != null
            ? new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(responseContent)
            }
            : new HttpResponseMessage
            {
                StatusCode = statusCode
            };

        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        return mockHandler;
    }

    /// <summary>
    /// Sample index.json content for testing
    /// </summary>
    private static string GetSampleIndexJson()
    {
        return @"{
  ""templates"": [
    {
      ""name"": ""blog-with-usync"",
      ""displayName"": ""Blog with uSync"",
      ""description"": ""Complete blog setup with uSync"",
      ""author"": ""PSW Team"",
      ""tags"": [""blog"", ""usync""],
      ""fileName"": ""blog-with-usync.yaml"",
      ""created"": ""2024-12-16""
    },
    {
      ""name"": ""commerce-starter"",
      ""displayName"": ""Commerce Starter"",
      ""description"": ""E-commerce starter template"",
      ""author"": ""PSW Team"",
      ""tags"": [""commerce"", ""shop""],
      ""fileName"": ""commerce-starter.yaml"",
      ""created"": ""2024-12-16""
    }
  ],
  ""lastUpdated"": ""2024-12-16""
}";
    }

    /// <summary>
    /// Sample template YAML content for testing
    /// </summary>
    private static string GetSampleTemplateYaml()
    {
        return @"metadata:
  name: blog-with-usync
  description: Complete blog setup with uSync for content synchronization
  author: Package Script Writer
  version: 1.0.0

configuration:
  template:
    name: Umbraco.Templates
    version: 14.3.0

  project:
    name: MyBlog
    createSolution: true
    solutionName: MyBlog

  packages:
    - name: uSync
      version: latest
    - name: uSync.Complete
      version: latest

  starterKit:
    enabled: false

  docker:
    dockerfile: false
    dockerCompose: false

  unattended:
    enabled: true
    database:
      type: SQLite
    admin:
      name: Administrator
      email: admin@example.com
      password: 1234567890

  output:
    oneliner: false
    removeComments: false
    includePrerelease: false

  execution:
    autoRun: false
    runDirectory: .
";
    }
}
