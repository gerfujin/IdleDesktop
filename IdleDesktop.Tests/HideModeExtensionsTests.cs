using IdleDesktop.Settings;

namespace IdleDesktop.Tests;

public class HideModeExtensionsTests
{
    // Pins the mapping from each mode to the parts it hides, so adding a mode forces a decision here.
    // The mode is passed by name because HideMode is internal and cannot appear in a public test signature.
    [Theory]
    [InlineData(nameof(HideMode.Icons), true, false)]
    [InlineData(nameof(HideMode.Cursor), false, true)]
    [InlineData(nameof(HideMode.Both), true, true)]
    public void HideMode_EachValue_HidesExpectedParts(string modeName, bool hidesIcons, bool hidesCursor)
    {
        // Arrange
        var mode = Enum.Parse<HideMode>(modeName);

        // Act
        var icons = mode.HidesIcons();
        var cursor = mode.HidesCursor();

        // Assert
        Assert.Equal(hidesIcons, icons);
        Assert.Equal(hidesCursor, cursor);
    }
}
