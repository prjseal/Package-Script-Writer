using FluentAssertions;
using PackageCliTool.Models.Cache;
using Xunit;

namespace PackageCliTool.Tests;

/// <summary>
/// Unit tests for CacheEntry model
/// </summary>
public class CacheEntryTests
{
    [Fact]
    public void IsValid_WithFutureExpirationDate_ReturnsTrue()
    {
        // Arrange
        var entry = new CacheEntry<string>
        {
            Key = "test-key",
            Data = "test-data",
            CachedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        // Act
        var isValid = entry.IsValid();

        // Assert
        isValid.Should().BeTrue("the entry expires in the future");
    }

    [Fact]
    public void IsValid_WithPastExpirationDate_ReturnsFalse()
    {
        // Arrange
        var entry = new CacheEntry<string>
        {
            Key = "test-key",
            Data = "test-data",
            CachedAt = DateTime.UtcNow.AddHours(-2),
            ExpiresAt = DateTime.UtcNow.AddHours(-1)
        };

        // Act
        var isValid = entry.IsValid();

        // Assert
        isValid.Should().BeFalse("the entry has expired");
    }

    [Fact]
    public void IsValid_WithExpirationAtCurrentTime_ReturnsFalse()
    {
        // Arrange
        var currentTime = DateTime.UtcNow;
        var entry = new CacheEntry<string>
        {
            Key = "test-key",
            Data = "test-data",
            CachedAt = currentTime.AddHours(-1),
            ExpiresAt = currentTime
        };

        // Act
        var isValid = entry.IsValid();

        // Assert
        isValid.Should().BeFalse("the entry expires exactly at current time");
    }

    [Fact]
    public void TimeRemaining_WithFutureExpiration_ReturnsCorrectDuration()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var expiresAt = now.AddHours(2);
        var entry = new CacheEntry<string>
        {
            Key = "test-key",
            Data = "test-data",
            CachedAt = now,
            ExpiresAt = expiresAt
        };

        // Act
        var remaining = entry.TimeRemaining();

        // Assert
        remaining.Should().BeGreaterThan(TimeSpan.FromHours(1.9), "approximately 2 hours remain");
        remaining.Should().BeLessThanOrEqualTo(TimeSpan.FromHours(2), "no more than 2 hours remain");
    }

    [Fact]
    public void TimeRemaining_WithPastExpiration_ReturnsZero()
    {
        // Arrange
        var entry = new CacheEntry<string>
        {
            Key = "test-key",
            Data = "test-data",
            CachedAt = DateTime.UtcNow.AddHours(-2),
            ExpiresAt = DateTime.UtcNow.AddHours(-1)
        };

        // Act
        var remaining = entry.TimeRemaining();

        // Assert
        remaining.Should().Be(TimeSpan.Zero, "expired entries have no time remaining");
    }

    [Fact]
    public void TimeRemaining_WithExpirationAtCurrentTime_ReturnsZero()
    {
        // Arrange
        var currentTime = DateTime.UtcNow;
        var entry = new CacheEntry<string>
        {
            Key = "test-key",
            Data = "test-data",
            CachedAt = currentTime.AddHours(-1),
            ExpiresAt = currentTime
        };

        // Act
        var remaining = entry.TimeRemaining();

        // Assert
        remaining.Should().Be(TimeSpan.Zero, "entries expiring now have no time remaining");
    }

    [Fact]
    public void CacheEntry_WithDefaultValues_InitializesCorrectly()
    {
        // Act
        var entry = new CacheEntry<string>();

        // Assert
        entry.Data.Should().BeNull();
        entry.Key.Should().Be(string.Empty);
        entry.CachedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        entry.ExpiresAt.Should().Be(default(DateTime));
    }

    [Theory]
    [InlineData(1)] // 1 hour
    [InlineData(24)] // 1 day
    [InlineData(168)] // 1 week
    public void CacheEntry_WithVariousTTLs_CalculatesExpirationCorrectly(int hoursUntilExpiration)
    {
        // Arrange
        var now = DateTime.UtcNow;
        var entry = new CacheEntry<string>
        {
            Key = "test-key",
            Data = "test-data",
            CachedAt = now,
            ExpiresAt = now.AddHours(hoursUntilExpiration)
        };

        // Act
        var isValid = entry.IsValid();
        var remaining = entry.TimeRemaining();

        // Assert
        isValid.Should().BeTrue("entry should be valid for the TTL period");
        remaining.TotalHours.Should().BeApproximately(hoursUntilExpiration, 0.1,
            $"should have approximately {hoursUntilExpiration} hours remaining");
    }

    [Fact]
    public void CacheEntry_SupportsGenericTypes()
    {
        // Arrange & Act
        var stringEntry = new CacheEntry<string>
        {
            Data = "test string",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var intEntry = new CacheEntry<int>
        {
            Data = 42,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var objectEntry = new CacheEntry<object>
        {
            Data = new { Name = "Test" },
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        // Assert
        stringEntry.Data.Should().Be("test string");
        intEntry.Data.Should().Be(42);
        objectEntry.Data.Should().NotBeNull();
    }

    [Fact]
    public void CacheEntry_WithShortTTL_ExpiresQuickly()
    {
        // Arrange
        var entry = new CacheEntry<string>
        {
            Key = "test-key",
            Data = "test-data",
            CachedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMilliseconds(100)
        };

        // Act - Before expiration
        var validBefore = entry.IsValid();

        // Wait for expiration
        Thread.Sleep(150);

        var validAfter = entry.IsValid();

        // Assert
        validBefore.Should().BeTrue("entry should be valid initially");
        validAfter.Should().BeFalse("entry should expire after waiting");
    }
}
