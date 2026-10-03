using System.Diagnostics;

namespace IdleDesktop;

internal static class DesktopIcons
{
    public static void Hide() => SetVisible(NativeMethods.SW_HIDE);

    public static void Show() => SetVisible(NativeMethods.SW_SHOW);

    private static void SetVisible(int nCmdShow)
    {
        var listView = FindIconListView();
        if (listView == IntPtr.Zero)
        {
            Debug.WriteLine("DesktopIcons: desktop icon list view (SysListView32) not found");
            return;
        }

        NativeMethods.ShowWindow(listView, nCmdShow);
    }

    // Explorer hosts desktop icons as Progman > SHELLDLL_DefView > SysListView32.
    // Once wallpaper animation / Win+Tab has run, SHELLDLL_DefView may instead be
    // reparented under one of the top-level WorkerW windows, so those are searched too.
    private static IntPtr FindIconListView()
    {
        var progman = NativeMethods.FindWindow("Progman", null);
        var defView = NativeMethods.FindWindowEx(progman, IntPtr.Zero, "SHELLDLL_DefView", null);

        var workerW = IntPtr.Zero;
        while (defView == IntPtr.Zero)
        {
            workerW = NativeMethods.FindWindowEx(IntPtr.Zero, workerW, "WorkerW", null);
            if (workerW == IntPtr.Zero)
                return IntPtr.Zero;
            defView = NativeMethods.FindWindowEx(workerW, IntPtr.Zero, "SHELLDLL_DefView", null);
        }

        return NativeMethods.FindWindowEx(defView, IntPtr.Zero, "SysListView32", null);
    }
}
