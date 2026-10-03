using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using PSW.Shared.Services;
using Xunit;

namespace PackageCliTool.Tests;

/// <summary>
/// Unit tests for PackageCompatibilityService, using canned NuGet registration responses
/// </summary>
public class PackageCompatibilityServiceTests
{
    private const string BaseUrl = "https://api.nuget.org/v3/registration5-gz-semver2/";

    [Theory]
    [InlineData("17.7.0", "7.0.8")]
    [InlineData("18.2.0", "8.0.1")]
    [InlineData("13.8.0", "4.2.2")]
    public async Task GetCompatibleVersionAsync_ReturnsNewestVersionMatchingUmbracoDependency(string umbracoVersion, string expected)
    {
        var service = CreateService(new Dictionary<string, object>
        {
            ["clean"] = Index(
                Leaf("4.2.2", ("Umbraco.Cms.Web.BackOffice", "[13.0.0, )")),
                Leaf("7.0.7", ("Umbraco.Cms.Web.Website", "[17.0.0, )")),
                Leaf("7.0.8", ("Umbraco.Cms.Web.Website", "[17.5.1, )")),
                Leaf("8.0.0-rc1", ("Umbraco.Cms.Web.Website", "[18.0.0-rc1, )")),
                Leaf("8.0.1", ("Umbraco.Cms.Web.Website", "[18.0.1, )")))
        });

        var result = await service.GetCompatibleVersionAsync("clean", umbracoVersion);

        result.Status.Should().Be(PackageCompatibilityStatus.Compatible);
        result.Version.Should().Be(expected);
    }

    [Fact]
    public async Task GetCompatibleVersionAsync_RespectsFullVersionOfLowerBound()
    {
        // 7.0.8 needs Umbraco 17.5.1, so Umbraco 17.0.0 gets 7.0.7
        var service = CreateService(new Dictionary<string, object>
        {
            ["clean"] = Index(
                Leaf("7.0.7", ("Umbraco.Cms.Web.Website", "[17.0.0, )")),
                Leaf("7.0.8", ("Umbraco.Cms.Web.Website", "[17.5.1, )")))
        });

        var result = await service.GetCompatibleVersionAsync("clean", "17.0.0");

        result.Version.Should().Be("7.0.7");
    }

    [Fact]
    public async Task GetCompatibleVersionAsync_SkipsUnlistedAndPrereleaseVersions()
    {
        var service = CreateService(new Dictionary<string, object>
        {
            ["pkg"] = Index(
                Leaf("1.0.0", ("Umbraco.Cms.Core", "[17.0.0, 18.0.0)")),
                Leaf("1.1.0", listed: false, ("Umbraco.Cms.Core", "[17.0.0, 18.0.0)")),
                Leaf("1.2.0-beta", ("Umbraco.Cms.Core", "[17.0.0, 18.0.0)")))
        });

        var result = await service.GetCompatibleVersionAsync("pkg", "17.7.0");

        result.Version.Should().Be("1.0.0");
    }

    [Fact]
    public async Task GetCompatibleVersionAsync_MatchesAnyDependencyGroup()
    {
        // Multi-targeted package: one group per Umbraco major
        var service = CreateService(new Dictionary<string, object>
        {
            ["Articulate"] = Index(
                Leaf("6.0.0", ("Umbraco.Cms.Web.Website", "[17.4.0, 18.0.0)"), ("Umbraco.Cms.Web.Website", "[16.5.1, 17.0.0)")))
        });

        var result = await service.GetCompatibleVersionAsync("Articulate", "16.5.1");

        result.Version.Should().Be("6.0.0");
    }

    [Fact]
    public async Task GetCompatibleVersionAsync_ResolvesMetaPackageThroughDependencies()
    {
        // uSync has no direct Umbraco.Cms dependency; uSync.BackOffice does
        var service = CreateService(new Dictionary<string, object>
        {
            ["uSync"] = Index(
                Leaf("13.2.0", ("uSync.BackOffice", "[13.2.0, )")),
                Leaf("17.1.0", ("uSync.BackOffice", "[17.1.0, )"))),
            ["uSync.BackOffice"] = Index(
                Leaf("13.2.0", ("Umbraco.Cms.Web.BackOffice", "[13.0.0, 14.0.0)")),
                Leaf("17.1.0", ("Umbraco.Cms.Api.Management", "[17.0.0, 18.0.0)")))
        });

        var result = await service.GetCompatibleVersionAsync("uSync", "13.8.0");

        result.Version.Should().Be("13.2.0");
    }

    [Fact]
    public async Task GetCompatibleVersionAsync_FetchesPagesThatAreNotInlined()
    {
        var service = CreateService(new Dictionary<string, object>
        {
            ["paged"] = new { items = new[] { new Dictionary<string, string> { ["@id"] = $"{BaseUrl}paged/page/1.json" } } },
            ["paged/page/1.json"] = new { items = new[] { Leaf("2.0.0", ("Umbraco.Cms.Core", "[17.0.0, )")) } }
        });

        var result = await service.GetCompatibleVersionAsync("paged", "17.7.0");

        result.Version.Should().Be("2.0.0");
    }

    [Fact]
    public async Task GetCompatibleVersionAsync_UsesButDoesNotCachePartialVersionList_WhenPageFails()
    {
        // Page 2 (the newer versions) returns 404
        var logger = new ListLogger();
        var service = CreateService(new Dictionary<string, object>
        {
            ["paged"] = new
            {
                items = new[]
                {
                    new Dictionary<string, string> { ["@id"] = $"{BaseUrl}paged/page/1.json" },
                    new Dictionary<string, string> { ["@id"] = $"{BaseUrl}paged/page/2.json" }
                }
            },
            ["paged/page/1.json"] = new { items = new[] { Leaf("2.0.0", ("Umbraco.Cms.Core", "[17.0.0, )")) } }
        }, logger);

        var first = await service.GetCompatibleVersionAsync("paged", "17.7.0");
        var second = await service.GetCompatibleVersionAsync("paged", "17.7.0");

        first.Version.Should().Be("2.0.0");
        second.Version.Should().Be("2.0.0");
        // Refetched on the second call, so the failed page is retried rather than served from cache
        logger.Warnings.Should().HaveCount(2).And.OnlyContain(x => x.Contains("page/2.json"));
    }

    [Fact]
    public async Task GetCompatibleVersionAsync_CachesCompleteVersionList()
    {
        var requests = 0;
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(new RoutingHandler(new Dictionary<string, string>
            {
                [$"{BaseUrl}clean/index.json"] = JsonSerializer.Serialize(Index(Leaf("7.0.8", ("Umbraco.Cms.Web.Website", "[17.5.1, )"))))
            }, () => requests++)));
        var service = new PackageCompatibilityService(httpClientFactory.Object, new MemoryCache(new MemoryCacheOptions()));

        await service.GetCompatibleVersionAsync("clean", "17.7.0");
        await service.GetCompatibleVersionAsync("clean", "17.7.0");

        requests.Should().Be(1);
    }

    [Fact]
    public async Task GetCompatibleVersionAsync_ReturnsNoCompatibleVersion_WhenNoRangeMatches()
    {
        var service = CreateService(new Dictionary<string, object>
        {
            ["clean"] = Index(Leaf("8.0.1", ("Umbraco.Cms.Web.Website", "[18.0.1, )")))
        });

        var result = await service.GetCompatibleVersionAsync("clean", "17.7.0");

        result.Status.Should().Be(PackageCompatibilityStatus.NoCompatibleVersion);
    }

    [Fact]
    public async Task GetCompatibleVersionAsync_ReturnsUnknown_WhenPackageHasNoUmbracoDependency()
    {
        var service = CreateService(new Dictionary<string, object>
        {
            ["Newtonsoft.Json"] = Index(Leaf("13.0.3"))
        });

        var result = await service.GetCompatibleVersionAsync("Newtonsoft.Json", "17.7.0");

        result.Status.Should().Be(PackageCompatibilityStatus.Unknown);
    }

    [Theory]
    [InlineData("missing", "17.7.0")]
    [InlineData("clean", "LTS")]
    public async Task GetCompatibleVersionAsync_ReturnsUnknown_WhenPackageMissingOrVersionInvalid(string packageId, string umbracoVersion)
    {
        var service = CreateService(new Dictionary<string, object>
        {
            ["clean"] = Index(Leaf("8.0.1", ("Umbraco.Cms.Web.Website", "[18.0.1, )")))
        });

        var result = await service.GetCompatibleVersionAsync(packageId, umbracoVersion);

        result.Status.Should().Be(PackageCompatibilityStatus.Unknown);
    }

    [Fact]
    public async Task GetCompatibleVersionAsync_LogsWarning_WhenNuGetReturnsError()
    {
        var logger = new ListLogger();
        var service = CreateService(new Dictionary<string, object>(), logger);

        await service.GetCompatibleVersionAsync("missing", "17.7.0");

        logger.Warnings.Should().ContainSingle()
            .Which.Should().Contain("missing").And.Contain("404");
    }

    [Fact]
    public async Task GetCompatibleVersionAsync_LogsWarning_WhenNuGetUnreachable()
    {
        var logger = new ListLogger();
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(new ThrowingHandler()));
        var service = new PackageCompatibilityService(httpClientFactory.Object, new MemoryCache(new MemoryCacheOptions()), logger);

        var result = await service.GetCompatibleVersionAsync("clean", "17.7.0");

        result.Status.Should().Be(PackageCompatibilityStatus.Unknown);
        logger.Warnings.Should().ContainSingle()
            .Which.Should().Be("NuGet compatibility lookup for clean failed: HttpRequestException: No such host is known.");
    }

    [Fact]
    public async Task GetCompatibleVersionAsync_LogsNothing_WhenLookupSucceeds()
    {
        var logger = new ListLogger();
        var service = CreateService(new Dictionary<string, object>
        {
            ["clean"] = Index(Leaf("7.0.8", ("Umbraco.Cms.Web.Website", "[17.5.1, )")))
        }, logger);

        await service.GetCompatibleVersionAsync("clean", "17.7.0");

        logger.Warnings.Should().BeEmpty();
    }

    private static object Index(params object[] leaves) => new { items = new[] { new { items = leaves } } };

    private static object Leaf(string version, params (string Id, string Range)[] dependencies) => Leaf(version, true, dependencies);

    private static object Leaf(string version, bool listed, params (string Id, string Range)[] dependencies) => new
    {
        catalogEntry = new
        {
            version,
            listed,
            dependencyGroups = dependencies.Select(x => new { dependencies = new[] { new { id = x.Id, range = x.Range } } })
        }
    };

    /// <summary>
    /// Serves each response gzipped, as the registration5-gz endpoints do. Keys are relative to the registration base URL;
    /// a bare package id maps to its index.json.
    /// </summary>
    private static PackageCompatibilityService CreateService(Dictionary<string, object> responses, ILogger<PackageCompatibilityService>? logger = null)
    {
        var routes = responses.ToDictionary(
            x => x.Key.Contains('/') ? $"{BaseUrl}{x.Key}" : $"{BaseUrl}{x.Key.ToLowerInvariant()}/index.json",
            x => JsonSerializer.Serialize(x.Value));

        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(new RoutingHandler(routes)));

        return new PackageCompatibilityService(httpClientFactory.Object, new MemoryCache(new MemoryCacheOptions()), logger);
    }

    private sealed class RoutingHandler(Dictionary<string, string> routes, Action? onRequest = null) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            onRequest?.Invoke();

            if (!routes.TryGetValue(request.RequestUri!.ToString(), out var json))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }

            using var buffer = new MemoryStream();
            using (var gzip = new GZipStream(buffer, CompressionMode.Compress, leaveOpen: true))
            {
                gzip.Write(Encoding.UTF8.GetBytes(json));
            }

            var content = new ByteArrayContent(buffer.ToArray());
            content.Headers.ContentEncoding.Add("gzip");
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        }
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            throw new HttpRequestException("No such host is known.");
    }

    private sealed class ListLogger : ILogger<PackageCompatibilityService>
    {
        public List<string> Warnings { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Warning) Warnings.Add(formatter(state, exception));
        }
    }
}
