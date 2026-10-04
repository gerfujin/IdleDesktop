using System.Diagnostics;

namespace IdleDesktop;

internal sealed class TrayApp : ApplicationContext
{
    private static readonly TimeSpan IdleThreshold = TimeSpan.FromSeconds(30);

    private readonly NotifyIcon _trayIcon;
    private readonly Icon? _appIcon;
    private readonly System.Windows.Forms.Timer _timer;
    private DateTime _lastLog = DateTime.MinValue;
    private bool _iconsHidden;

    public TrayApp()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Exit", null, (_, _) => Exit());

        // Request the small-icon size so the matching frame is picked instead of a downscaled large one.
        using (var stream = typeof(TrayApp).Assembly.GetManifestResourceStream("app.ico"))
        {
            if (stream != null)
                _appIcon = new Icon(stream, SystemInformation.SmallIconSize);
        }

        _trayIcon = new NotifyIcon
        {
            Icon = _appIcon ?? SystemIcons.Application,
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
        var state = SystemState.Read();

        var action = IdleLogic.Decide(state.Idle, IdleThreshold, state.ForegroundClass, _iconsHidden);
        switch (action)
        {
            case IconAction.Hide:
                DesktopIcons.Hide();
                _iconsHidden = true;
                Debug.WriteLine("icons hidden");
                break;
            case IconAction.Show:
                DesktopIcons.Show();
                _iconsHidden = false;
                Debug.WriteLine("icons shown");
                break;
        }

        var now = DateTime.Now;
        if (now - _lastLog < TimeSpan.FromSeconds(1))
            return;
        _lastLog = now;

        Debug.WriteLine(
            $"[{now:HH:mm:ss}] idle={(long)state.Idle.TotalMilliseconds}ms " +
            $"fg={state.ForegroundClass} desktop={IdleLogic.IsDesktopClass(state.ForegroundClass)} hidden={_iconsHidden}");
    }

    private void Exit()
    {
        DesktopIcons.Show();
        _timer.Stop();
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        _appIcon?.Dispose();
        ExitThread();
    }
}
