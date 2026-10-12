using System.Diagnostics;
using IdleDesktop.Settings;

namespace IdleDesktop;

/// <summary>
/// Tray application: polls <see cref="SystemState"/> on a timer, asks <see cref="IdleLogic"/> what to do,
/// and hides or shows the desktop icons and cursor via <see cref="DesktopIcons"/> and <see cref="SystemCursor"/>.
/// </summary>
internal sealed class TrayApp : ApplicationContext
{
    private AppSettings _settings;
    private SettingsForm? _settingsForm;
    private readonly NotifyIcon _trayIcon;
    private readonly Icon? _appIcon;
    private readonly System.Windows.Forms.Timer _timer;
    private DateTime _lastLog = DateTime.MinValue;
    private bool _iconsHidden;

    public TrayApp()
    {
        _settings = SettingsStore.Load();
        Debug.WriteLine($"Settings: {_settings}");

        var menu = new ContextMenuStrip();
        menu.Items.Add("Settings…", null, (_, _) => OpenSettings());
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
        _trayIcon.DoubleClick += (_, _) => OpenSettings();

        _timer = new System.Windows.Forms.Timer { Interval = 250 };
        _timer.Tick += OnTick;
        _timer.Start();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        var state = SystemState.Read();

        var action = IdleLogic.Decide(state.Idle, _settings.IdleThreshold, state.ForegroundClass, _iconsHidden);
        switch (action)
        {
            case IconAction.Hide:
                if (_settings.Mode.HidesIcons())
                    DesktopIcons.Hide();
                if (_settings.Mode.HidesCursor())
                    SystemCursor.Hide();
                _iconsHidden = true;
                Debug.WriteLine("icons hidden");
                break;
            case IconAction.Show:
                DesktopIcons.Show();
                SystemCursor.Show();
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

    private void OpenSettings()
    {
        // Double-click on the tray icon can arrive while the modal dialog is already open.
        if (_settingsForm != null)
        {
            _settingsForm.Activate();
            return;
        }

        AppSettings? result;
        using (_settingsForm = new SettingsForm(_settings) { Icon = _appIcon ?? SystemIcons.Application })
        {
            _settingsForm.ShowDialog();
            result = _settingsForm.Result;
        }
        _settingsForm = null;

        if (result == null)
            return;

        try
        {
            SettingsStore.Save(result);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not save settings: {ex.Message}", "IdleDesktop",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        // Apply even if saving failed; the next tick decides again under the new rules.
        _settings = result;
        Debug.WriteLine($"Settings: {_settings}");
        DesktopIcons.Show();
        SystemCursor.Show();
        _iconsHidden = false;
    }

    private void Exit()
    {
        DesktopIcons.Show();
        SystemCursor.Show();
        _timer.Stop();
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        _appIcon?.Dispose();
        ExitThread();
    }
}
