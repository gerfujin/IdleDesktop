using IdleDesktop.Settings;

namespace IdleDesktop.Tests;

public class SettingsStoreTests
{
    // No file content at all must behave like a first run.
    [Fact]
    public void Parse_Null_ReturnsDefault()
    {
        // Act
        var result = SettingsStore.Parse(null);

        // Assert
        Assert.Equal(AppSettings.Default, result);
    }

    // An empty file (e.g. truncated by a crash) must not throw.
    [Fact]
    public void Parse_EmptyString_ReturnsDefault()
    {
        // Act
        var result = SettingsStore.Parse("");

        // Assert
        Assert.Equal(AppSettings.Default, result);
    }

    // A hand-edited or corrupted file must not stop the app from starting.
    [Fact]
    public void Parse_InvalidJson_ReturnsDefault()
    {
        // Act
        var result = SettingsStore.Parse("{ not json");

        // Assert
        Assert.Equal(AppSettings.Default, result);
    }

    // Happy path, also checks that Mode is read from a string and property names are case-insensitive.
    [Fact]
    public void Parse_ValidJson_ReturnsValues()
    {
        // Arrange
        var json = """{"IdleSeconds": 60, "mode": "Cursor"}""";

        // Act
        var result = SettingsStore.Parse(json);

        // Assert
        Assert.Equal(new AppSettings(60, HideMode.Cursor), result);
    }

    // A mode from a newer or edited file is unknown to us; fall back rather than guess.
    [Fact]
    public void Parse_UnknownMode_ReturnsDefault()
    {
        // Arrange
        var json = """{"IdleSeconds": 60, "Mode": "Banana"}""";

        // Act
        var result = SettingsStore.Parse(json);

        // Assert
        Assert.Equal(AppSettings.Default, result);
    }

    // A too-small value would make icons vanish almost instantly, so it is raised to the minimum.
    [Fact]
    public void Parse_IdleBelowMin_ClampsToMin()
    {
        // Act
        var result = SettingsStore.Parse("""{"IdleSeconds": 1, "Mode": "Icons"}""");

        // Assert
        Assert.Equal(AppSettings.MinIdleSeconds, result.IdleSeconds);
    }

    // An absurdly large value is capped at the maximum instead of being rejected.
    [Fact]
    public void Parse_IdleAboveMax_ClampsToMax()
    {
        // Act
        var result = SettingsStore.Parse("""{"IdleSeconds": 99999, "Mode": "Icons"}""");

        // Assert
        Assert.Equal(AppSettings.MaxIdleSeconds, result.IdleSeconds);
    }

    // Guards that what Save writes is exactly what Load reads back.
    [Fact]
    public void Serialize_ThenParse_ReturnsSameSettings()
    {
        // Arrange
        var original = new AppSettings(120, HideMode.Icons);

        // Act
        var result = SettingsStore.Parse(SettingsStore.Serialize(original));

        // Assert
        Assert.Equal(original, result);
    }
}
