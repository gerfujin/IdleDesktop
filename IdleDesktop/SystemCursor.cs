namespace IdleDesktop;

/// <summary>
/// Hides and restores the mouse cursor system-wide. Calls the cursor functions in <see cref="NativeMethods"/>.
/// </summary>
internal static class SystemCursor
{
    // OCR_* ids of the standard system cursors that SetSystemCursor can replace.
    private enum CursorId : uint
    {
        Normal = 32512,
        IBeam = 32513,
        Wait = 32514,
        Cross = 32515,
        Up = 32516,
        SizeNwse = 32642,
        SizeNesw = 32643,
        SizeWe = 32644,
        SizeNs = 32645,
        SizeAll = 32646,
        No = 32648,
        Hand = 32649,
        AppStarting = 32650,
        Help = 32651
    }

    // ShowCursor is not used: it only affects windows of the calling process.
    // Instead every standard cursor is replaced with a fully transparent one.
    public static void Hide()
    {
        int width = NativeMethods.GetSystemMetrics(NativeMethods.SM_CXCURSOR);
        int height = NativeMethods.GetSystemMetrics(NativeMethods.SM_CYCURSOR);

        // 1 bit per pixel, rows padded to 16 bits. AND = 1 and XOR = 0 leaves the screen pixel unchanged.
        int size = (width + 15) / 16 * 2 * height;

        foreach (var id in Enum.GetValues<CursorId>())
        {
            // A new cursor per id is required: SetSystemCursor takes ownership of the handle and destroys it.
            var andPlane = new byte[size];
            Array.Fill(andPlane, (byte)0xFF);
            var xorPlane = new byte[size];

            var cursor = NativeMethods.CreateCursor(IntPtr.Zero, 0, 0, width, height, andPlane, xorPlane);
            if (cursor != IntPtr.Zero)
                NativeMethods.SetSystemCursor(cursor, (uint)id);
        }
    }

    // Reloading the user's cursor scheme from the registry restores all replaced cursors.
    // Without SPIF_UPDATEINIFILE the registry is not modified, and it is harmless if nothing was hidden.
    public static void Show()
    {
        NativeMethods.SystemParametersInfo(NativeMethods.SPI_SETCURSORS, 0, IntPtr.Zero, 0);
    }
}
