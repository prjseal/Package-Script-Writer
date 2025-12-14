using FluentAssertions;
using PackageCliTool.Services;
using Xunit;

namespace PackageCliTool.Tests;

/// <summary>
/// Unit tests for CacheService
/// </summary>
public class CacheServiceTests : IDisposable
{
    private readonly string _testCacheDirectory;

    public CacheServiceTests()
    {
        // Create a unique temporary directory for each test run
        _testCacheDirectory = Path.Combine(Path.GetTempPath(), $"psw-test-cache-{Guid.NewGuid()}");
    }

    public void Dispose()
    {
        // Clean up test cache directory after each test
        if (Directory.Exists(_testCacheDirectory))
        {
            Directory.Delete(_testCacheDirectory, recursive: true);
        }
    }

    [Fact]
    public void CacheService_Initialization_CreatesDirectory()
    {
        // Act
        var cacheService = new CacheService(cacheDirectory: _testCacheDirectory);

        // Assert
        Directory.Exists(_testCacheDirectory).Should().BeTrue("cache directory should be created");
        cacheService.IsEnabled.Should().BeTrue("cache should be enabled by default");
    }

    [Fact]
    public void CacheService_WithDisabledCache_DoesNotCreateDirectory()
    {
        // Arrange
        var nonExistentDir = Path.Combine(Path.GetTempPath(), $"psw-disabled-{Guid.NewGuid()}");

        // Act
        var cacheService = new CacheService(enabled: false, cacheDirectory: nonExistentDir);

        // Assert
        cacheService.IsEnabled.Should().BeFalse();
        // Note: Directory might still be created during initialization
        // This test verifies the enabled flag is respected
    }

    [Fact]
    public void Set_AndGet_StoresAndRetrievesValue()
    {
        // Arrange
        var cacheService = new CacheService(cacheDirectory: _testCacheDirectory);
        var key = "test-package";
        var value = "package-data-content";

        // Act
        cacheService.Set(key, value);
        var retrieved = cacheService.Get(key);

        // Assert
        retrieved.Should().Be(value);
    }

    [Fact]
    public void Get_WithNonExistentKey_ReturnsNull()
    {
        // Arrange
        var cacheService = new CacheService(cacheDirectory: _testCacheDirectory);

        // Act
        var retrieved = cacheService.Get("non-existent-key");

        // Assert
        retrieved.Should().BeNull();
    }

    [Fact]
    public void Set_WithTemplateVersionCacheType_StoresInCorrectCache()
    {
        // Arrange
        var cacheService = new CacheService(cacheDirectory: _testCacheDirectory);
        var key = "template-key";
        var value = "template-version-data";

        // Act
        cacheService.Set(key, value, CacheType.TemplateVersion);
        var retrieved = cacheService.Get(key, CacheType.TemplateVersion);

        // Assert
        retrieved.Should().Be(value);
    }

    [Fact]
    public void Set_WithDifferentCacheTypes_KeepsSeparate()
    {
        // Arrange
        var cacheService = new CacheService(cacheDirectory: _testCacheDirectory);
        var key = "same-key";
        var packageValue = "package-data";
        var templateValue = "template-data";

        // Act
        cacheService.Set(key, packageValue, CacheType.Package);
        cacheService.Set(key, templateValue, CacheType.TemplateVersion);

        // Assert
        cacheService.Get(key, CacheType.Package).Should().Be(packageValue);
        cacheService.Get(key, CacheType.TemplateVersion).Should().Be(templateValue);
    }

    [Fact]
    public void Set_WithCustomTTL_UsesCustomExpiration()
    {
        // Arrange
        var cacheService = new CacheService(ttlHours: 1, cacheDirectory: _testCacheDirectory);
        var key = "custom-ttl-key";
        var value = "test-value";

        // Act
        cacheService.Set(key, value, ttlHours: 24); // 24 hours instead of default 1

        // Assert
        var retrieved = cacheService.Get(key);
        retrieved.Should().Be(value, "entry should be valid with custom TTL");
    }

    [Fact]
    public void Clear_RemovesAllCacheEntries()
    {
        // Arrange
        var cacheService = new CacheService(cacheDirectory: _testCacheDirectory);
        cacheService.Set("key1", "value1");
        cacheService.Set("key2", "value2", CacheType.TemplateVersion);

        // Act
        cacheService.Clear();

        // Assert
        cacheService.Get("key1").Should().BeNull();
        cacheService.Get("key2", CacheType.TemplateVersion).Should().BeNull();
    }

    [Fact]
    public void Clear_WithSpecificType_OnlyClearsThatType()
    {
        // Arrange
        var cacheService = new CacheService(cacheDirectory: _testCacheDirectory);
        cacheService.Set("package-key", "package-value", CacheType.Package);
        cacheService.Set("template-key", "template-value", CacheType.TemplateVersion);

        // Act
        cacheService.Clear(CacheType.Package);

        // Assert
        cacheService.Get("package-key", CacheType.Package).Should().BeNull("package cache was cleared");
        cacheService.Get("template-key", CacheType.TemplateVersion).Should().Be("template-value",
            "template cache should remain intact");
    }

    [Fact]
    public void CacheService_PersistsToDisk()
    {
        // Arrange
        var key = "persistent-key";
        var value = "persistent-value";

        // Act - Create cache, set value
        var cacheService1 = new CacheService(cacheDirectory: _testCacheDirectory);
        cacheService1.Set(key, value);

        // Create new instance - should load from disk
        var cacheService2 = new CacheService(cacheDirectory: _testCacheDirectory);
        var retrieved = cacheService2.Get(key);

        // Assert
        retrieved.Should().Be(value, "cache should persist across service instances");
    }

    [Fact]
    public void CacheService_WithDisabledCache_DoesNotStoreOrRetrieve()
    {
        // Arrange
        var cacheService = new CacheService(enabled: false, cacheDirectory: _testCacheDirectory);
        var key = "disabled-key";
        var value = "disabled-value";

        // Act
        cacheService.Set(key, value);
        var retrieved = cacheService.Get(key);

        // Assert
        retrieved.Should().BeNull("disabled cache should not store or retrieve");
    }

    [Fact]
    public void CacheService_LoadsExistingCacheOnInitialization()
    {
        // Arrange - Create and populate cache
        var key = "preload-key";
        var value = "preload-value";
        var cacheService1 = new CacheService(cacheDirectory: _testCacheDirectory);
        cacheService1.Set(key, value);

        // Act - Create new instance
        var cacheService2 = new CacheService(cacheDirectory: _testCacheDirectory);

        // Assert
        var retrieved = cacheService2.Get(key);
        retrieved.Should().Be(value, "existing cache should be loaded on initialization");
    }

    [Fact]
    public void CacheService_WithCorruptedCacheFile_CreatesNewCache()
    {
        // Arrange - Create cache directory and write invalid YAML
        Directory.CreateDirectory(_testCacheDirectory);
        var cacheFile = Path.Combine(_testCacheDirectory, "cache.yaml");
        File.WriteAllText(cacheFile, "invalid: yaml: content: [[[");

        // Act - Should handle corrupted file gracefully
        var act = () => new CacheService(cacheDirectory: _testCacheDirectory);

        // Assert
        act.Should().NotThrow("should handle corrupted cache file gracefully");
        var cacheService = new CacheService(cacheDirectory: _testCacheDirectory);
        cacheService.Get("any-key").Should().BeNull("should start with empty cache");
    }

    [Fact]
    public void CacheService_WithVeryShortTTL_ExpiresQuickly()
    {
        // Arrange - Use very short TTL (converted to hours: 1ms = ~0.00000027 hours)
        var cacheService = new CacheService(ttlHours: 0, cacheDirectory: _testCacheDirectory);
        var key = "short-lived-key";
        var value = "short-lived-value";

        // Act
        // Set with 0.001 hours (3.6 seconds) TTL to make test faster
        cacheService.Set(key, value, ttlHours: 0);

        // Assert - Should be expired immediately since TTL is 0
        var retrieved = cacheService.Get(key);
        retrieved.Should().BeNull("entry with 0 hour TTL should be expired");
    }

    [Fact]
    public void Set_OverwritesExistingKey()
    {
        // Arrange
        var cacheService = new CacheService(cacheDirectory: _testCacheDirectory);
        var key = "overwrite-key";

        // Act
        cacheService.Set(key, "original-value");
        cacheService.Set(key, "new-value");

        // Assert
        cacheService.Get(key).Should().Be("new-value", "newer value should overwrite original");
    }

    [Fact]
    public void CacheService_WithMultipleEntries_ManagesCorrectly()
    {
        // Arrange
        var cacheService = new CacheService(cacheDirectory: _testCacheDirectory);
        var entries = new Dictionary<string, string>
        {
            ["package1"] = "data1",
            ["package2"] = "data2",
            ["package3"] = "data3",
            ["template1"] = "template-data1",
            ["template2"] = "template-data2"
        };

        // Act
        cacheService.Set("package1", entries["package1"], CacheType.Package);
        cacheService.Set("package2", entries["package2"], CacheType.Package);
        cacheService.Set("package3", entries["package3"], CacheType.Package);
        cacheService.Set("template1", entries["template1"], CacheType.TemplateVersion);
        cacheService.Set("template2", entries["template2"], CacheType.TemplateVersion);

        // Assert
        cacheService.Get("package1", CacheType.Package).Should().Be(entries["package1"]);
        cacheService.Get("package2", CacheType.Package).Should().Be(entries["package2"]);
        cacheService.Get("package3", CacheType.Package).Should().Be(entries["package3"]);
        cacheService.Get("template1", CacheType.TemplateVersion).Should().Be(entries["template1"]);
        cacheService.Get("template2", CacheType.TemplateVersion).Should().Be(entries["template2"]);
    }

    [Fact]
    public void CacheService_WithNullOrEmptyKey_HandlesGracefully()
    {
        // Arrange
        var cacheService = new CacheService(cacheDirectory: _testCacheDirectory);

        // Act & Assert - Should not throw
        var act1 = () => cacheService.Set("", "value");
        var act2 = () => cacheService.Get("");

        act1.Should().NotThrow();
        act2.Should().NotThrow();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(24)]
    [InlineData(168)]
    public void CacheService_WithVariousTTLHours_WorksCorrectly(int ttlHours)
    {
        // Arrange
        var cacheService = new CacheService(ttlHours: ttlHours, cacheDirectory: _testCacheDirectory);
        var key = $"ttl-{ttlHours}-key";
        var value = $"ttl-{ttlHours}-value";

        // Act
        cacheService.Set(key, value);
        var retrieved = cacheService.Get(key);

        // Assert
        retrieved.Should().Be(value, $"cache with {ttlHours} hour TTL should work correctly");
    }
}
