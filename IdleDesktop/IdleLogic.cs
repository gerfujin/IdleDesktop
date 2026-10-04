namespace IdleDesktop;

internal enum IconAction { None, Hide, Show }

internal static class IdleLogic
{
    public static IconAction Decide(TimeSpan idle, TimeSpan threshold, string foregroundClass, bool iconsHidden)
    {
        if (!iconsHidden && idle > threshold && IsDesktopClass(foregroundClass))
            return IconAction.Hide;

        // Any input brings the icons back, regardless of which window is active.
        if (iconsHidden && idle < threshold)
            return IconAction.Show;

        return IconAction.None;
    }

    public static bool IsDesktopClass(string className)
    {
        return className is "Progman" or "WorkerW" or "Shell_TrayWnd";
    }

    public static uint ElapsedMs(uint now, uint lastInput)
    {
        // Both values are 32-bit tick counts that wrap every ~49.7 days;
        // unsigned subtraction still yields the correct difference across the wrap.
        return unchecked(now - lastInput);
    }
}
