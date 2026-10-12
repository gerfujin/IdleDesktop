using System.Runtime.InteropServices;
using System.Text;

namespace IdleDesktop;

/// <summary>
/// Snapshot of the idle time and foreground window class, read through <see cref="NativeMethods"/>
/// (the idle calculation uses <see cref="IdleLogic.ElapsedMs"/>).
/// </summary>
internal readonly record struct SystemState(TimeSpan Idle, string ForegroundClass)
{
    public static SystemState Read() => new(GetIdleTime(), GetForegroundClassName());

    private static TimeSpan GetIdleTime()
    {
        var info = new NativeMethods.LASTINPUTINFO
        {
            cbSize = (uint)Marshal.SizeOf<NativeMethods.LASTINPUTINFO>()
        };
        if (!NativeMethods.GetLastInputInfo(ref info))
            return TimeSpan.Zero;

        uint idleMs = IdleLogic.ElapsedMs((uint)Environment.TickCount, info.dwTime);
        return TimeSpan.FromMilliseconds(idleMs);
    }

    private static string GetForegroundClassName()
    {
        var hwnd = NativeMethods.GetForegroundWindow();
        if (hwnd == IntPtr.Zero)
            return string.Empty;

        var sb = new StringBuilder(256);
        return NativeMethods.GetClassName(hwnd, sb, sb.Capacity) > 0 ? sb.ToString() : string.Empty;
    }
}
