namespace IdleDesktop;

/// <summary>
/// Entry point: enforces a single instance, restores the cursor and icons if a previous run left them hidden,
/// and runs <see cref="TrayApp"/>.
/// </summary>
static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        using var mutex = new Mutex(true, "IdleDesktop_SingleInstance", out bool createdNew);
        if (!createdNew)
            return;

        // A previous run may have crashed or been killed with the cursor still hidden.
        SystemCursor.Show();

        // Let unhandled exceptions crash the app as usual, but restore the cursor and icons first.
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        AppDomain.CurrentDomain.UnhandledException += (_, _) =>
        {
            SystemCursor.Show();
            DesktopIcons.Show();
        };

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(new TrayApp());
    }
}
