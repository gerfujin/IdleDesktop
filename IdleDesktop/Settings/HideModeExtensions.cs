namespace IdleDesktop.Settings;

/// <summary>
/// Tells which parts (icons, cursor) a <see cref="HideMode"/> hides. Used by <see cref="TrayApp"/>.
/// </summary>
internal static class HideModeExtensions
{
    public static bool HidesIcons(this HideMode mode) => mode is HideMode.Icons or HideMode.Both;

    public static bool HidesCursor(this HideMode mode) => mode is HideMode.Cursor or HideMode.Both;
}
