namespace IdleDesktop.Tests;

public class IdleLogicTests
{
    private static readonly TimeSpan Threshold = TimeSpan.FromSeconds(30);

    // Every window class that makes up the desktop surface or taskbar must count as "on the desktop".
    [Theory]
    [InlineData("Progman")]
    [InlineData("WorkerW")]
    [InlineData("Shell_TrayWnd")]
    public void IsDesktopClass_DesktopOrTaskbar_ReturnsTrue(string className)
    {
        // Act
        var result = IdleLogic.IsDesktopClass(className);

        // Assert
        Assert.True(result);
    }

    // Regular apps and an empty class (no foreground window) must never trigger hiding.
    [Theory]
    [InlineData("Chrome_WidgetWin_1")]
    [InlineData("CabinetWClass")]
    [InlineData("SunAwtFrame")]
    [InlineData("")]
    public void IsDesktopClass_OtherWindow_ReturnsFalse(string className)
    {
        // Act
        var result = IdleLogic.IsDesktopClass(className);

        // Assert
        Assert.False(result);
    }

    // Baseline: with no wrap-around the result is a plain subtraction.
    [Fact]
    public void ElapsedMs_NormalCase_ReturnsDifference()
    {
        // Act
        var result = IdleLogic.ElapsedMs(now: 1000, lastInput: 400);

        // Assert
        Assert.Equal(600u, result);
    }

    // Input at this very tick means zero idle time, not a huge or negative value.
    [Fact]
    public void ElapsedMs_SameValues_ReturnsZero()
    {
        // Act
        var result = IdleLogic.ElapsedMs(now: 500, lastInput: 500);

        // Assert
        Assert.Equal(0u, result);
    }

    // The 32-bit tick counter wraps every ~49.7 days; a naive subtraction would break there.
    [Fact]
    public void ElapsedMs_CounterWrapped_ReturnsCorrectDifference()
    {
        // Arrange: last input was 2 ticks before the wrap (MaxValue - 1 -> MaxValue -> 0),
        // and now is 3 ticks after it (0 -> 1 -> 2 -> 3), so 2 + 3 = 5 ms elapsed.
        uint lastInput = uint.MaxValue - 1;
        uint now = 3;

        // Act
        var result = IdleLogic.ElapsedMs(now, lastInput);

        // Assert
        Assert.Equal(5u, result);
    }

    // The main happy path: idle long enough on the desktop hides the icons.
    [Fact]
    public void Decide_IdleOverThresholdOnDesktop_ReturnsHide()
    {
        // Arrange
        var idle = TimeSpan.FromSeconds(31);

        // Act
        var result = IdleLogic.Decide(idle, Threshold, "Progman", iconsHidden: false);

        // Assert
        Assert.Equal(IconAction.Hide, result);
    }

    // Pins the boundary as inclusive (>=), so an off-by-one change to > gets caught.
    [Fact]
    public void Decide_IdleEqualsThreshold_ReturnsHide()
    {
        // Act
        var result = IdleLogic.Decide(Threshold, Threshold, "Progman", iconsHidden: false);

        // Assert
        Assert.Equal(IconAction.Hide, result);
    }

    // The other side of the boundary: the smallest shortfall must not hide anything yet.
    [Fact]
    public void Decide_IdleJustBelowThreshold_ReturnsNone()
    {
        // Arrange
        var idle = Threshold - TimeSpan.FromMilliseconds(1);

        // Act
        var result = IdleLogic.Decide(idle, Threshold, "Progman", iconsHidden: false);

        // Assert
        Assert.Equal(IconAction.None, result);
    }

    // Idling while working in another app must not hide icons the user can't even see.
    [Fact]
    public void Decide_IdleOverThresholdInOtherApp_ReturnsNone()
    {
        // Arrange
        var idle = TimeSpan.FromSeconds(31);

        // Act
        var result = IdleLogic.Decide(idle, Threshold, "Chrome_WidgetWin_1", iconsHidden: false);

        // Assert
        Assert.Equal(IconAction.None, result);
    }

    // Once hidden, staying idle must not re-issue Hide on every 250 ms tick.
    [Fact]
    public void Decide_AlreadyHidden_ReturnsNone()
    {
        // Arrange
        var idle = TimeSpan.FromSeconds(31);

        // Act
        var result = IdleLogic.Decide(idle, Threshold, "Progman", iconsHidden: true);

        // Assert
        Assert.Equal(IconAction.None, result);
    }

    // Any input restores icons even if the user has switched to another window.
    [Fact]
    public void Decide_HiddenAndUserActive_ReturnsShow()
    {
        // Act
        var result = IdleLogic.Decide(TimeSpan.Zero, Threshold, "Chrome_WidgetWin_1", iconsHidden: true);

        // Assert
        Assert.Equal(IconAction.Show, result);
    }

    // An active user with visible icons is the steady state; nothing should happen.
    [Fact]
    public void Decide_VisibleAndUserActive_ReturnsNone()
    {
        // Act
        var result = IdleLogic.Decide(TimeSpan.Zero, Threshold, "Progman", iconsHidden: false);

        // Assert
        Assert.Equal(IconAction.None, result);
    }
}
