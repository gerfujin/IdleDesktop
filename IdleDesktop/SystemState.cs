using System.Runtime.InteropServices;
using System.Text;

namespace IdleDesktop;

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

        // Both values are 32-bit tick counts that wrap every ~49.7 days;
        // unsigned subtraction still yields the correct difference across the wrap.
        uint idleMs = unchecked((uint)Environment.TickCount - info.dwTime);
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
