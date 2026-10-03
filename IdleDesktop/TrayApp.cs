using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace IdleDesktop;

internal sealed class TrayApp : ApplicationContext
{
    private static readonly TimeSpan IdleThreshold = TimeSpan.FromSeconds(30);

    private readonly NotifyIcon _trayIcon;
    private readonly System.Windows.Forms.Timer _timer;
    private DateTime _lastLog = DateTime.MinValue;
    private bool _iconsHidden;

    public TrayApp()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Exit", null, (_, _) => Exit());

        _trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "IdleDesktop",
            Visible = true,
            ContextMenuStrip = menu
        };

        _timer = new System.Windows.Forms.Timer { Interval = 250 };
        _timer.Tick += OnTick;
        _timer.Start();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        var idle = GetIdleTime();
        var className = GetForegroundClassName();

        if (!_iconsHidden && idle >= IdleThreshold && IsDesktopClass(className))
        {
            DesktopIcons.Hide();
            _iconsHidden = true;
            Debug.WriteLine("icons hidden");
        }
        else if (_iconsHidden && idle < IdleThreshold)
        {
            // Any input brings the icons back, regardless of which window is active.
            DesktopIcons.Show();
            _iconsHidden = false;
            Debug.WriteLine("icons shown");
        }

        var now = DateTime.Now;
        if (now - _lastLog < TimeSpan.FromSeconds(1))
            return;
        _lastLog = now;

        Debug.WriteLine(
            $"[{now:HH:mm:ss}] idle={(long)idle.TotalMilliseconds}ms " +
            $"fg={className} desktop={IsDesktopClass(className)} hidden={_iconsHidden}");
    }

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

    private static bool IsDesktopClass(string className)
    {
        return className is "Progman" or "WorkerW" or "Shell_TrayWnd";
    }

    private void Exit()
    {
        DesktopIcons.Show();
        _timer.Stop();
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        ExitThread();
    }
}
