namespace IdleDesktop.Settings;

/// <summary>
/// Modal dialog for editing <see cref="AppSettings"/>, built entirely in code.
/// <see cref="Result"/> holds the new settings after Save and stays null if the dialog is cancelled.
/// </summary>
internal sealed class SettingsForm : Form
{
    private readonly NumericUpDown _idleSeconds;
    private readonly RadioButton _iconsOnly;
    private readonly RadioButton _cursorOnly;
    private readonly RadioButton _both;

    public AppSettings? Result { get; private set; }

    public SettingsForm(AppSettings current)
    {
        Text = "IdleDesktop Settings";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = true;
        StartPosition = FormStartPosition.CenterScreen;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(12);

        _idleSeconds = new NumericUpDown
        {
            Minimum = AppSettings.MinIdleSeconds,
            Maximum = AppSettings.MaxIdleSeconds,
            Value = Math.Clamp(current.IdleSeconds, AppSettings.MinIdleSeconds, AppSettings.MaxIdleSeconds),
            Anchor = AnchorStyles.Left,
            Margin = new Padding(3)
        };

        _iconsOnly = new RadioButton { Text = "Desktop icons", AutoSize = true };
        _cursorOnly = new RadioButton { Text = "Mouse cursor", AutoSize = true };
        _both = new RadioButton { Text = "Both", AutoSize = true };
        (current.Mode switch
        {
            HideMode.Icons => _iconsOnly,
            HideMode.Cursor => _cursorOnly,
            _ => _both
        }).Checked = true;

        var modeLayout = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Fill
        };
        modeLayout.Controls.AddRange([_iconsOnly, _cursorOnly, _both]);

        var modeGroup = new GroupBox
        {
            Text = "What to hide",
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Fill,
            Padding = new Padding(9),
            Margin = new Padding(3, 12, 3, 12)
        };
        modeGroup.Controls.Add(modeLayout);

        var save = new Button { Text = "Save", AutoSize = true, Margin = new Padding(3) };
        save.Click += (_, _) => Save();
        var cancel = new Button
        {
            Text = "Cancel",
            AutoSize = true,
            Margin = new Padding(3),
            DialogResult = DialogResult.Cancel
        };

        var buttons = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Anchor = AnchorStyles.Right
        };
        buttons.Controls.AddRange([save, cancel]);

        var layout = new TableLayoutPanel
        {
            ColumnCount = 2,
            RowCount = 3,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Fill
        };
        layout.Controls.Add(new Label { Text = "Hide after (seconds):", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        layout.Controls.Add(_idleSeconds, 1, 0);
        layout.Controls.Add(modeGroup, 0, 1);
        layout.SetColumnSpan(modeGroup, 2);
        layout.Controls.Add(buttons, 0, 2);
        layout.SetColumnSpan(buttons, 2);
        Controls.Add(layout);

        AcceptButton = save;
        CancelButton = cancel;
    }

    private void Save()
    {
        var mode = _iconsOnly.Checked ? HideMode.Icons
            : _cursorOnly.Checked ? HideMode.Cursor
            : HideMode.Both;

        Result = new AppSettings((int)_idleSeconds.Value, mode);
        DialogResult = DialogResult.OK;
    }
}
