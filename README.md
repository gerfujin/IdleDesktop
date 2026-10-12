# IdleDesktop

![CI](https://github.com/gerfujin/IdleDesktop/actions/workflows/ci.yml/badge.svg)

A tiny Windows tray utility that hides desktop icons when you sit idle on the desktop and brings them back on any input.

## Features

- Hides the desktop icons and/or the mouse cursor after a period of inactivity while the desktop (or taskbar) is the active window
- The idle threshold is configurable from 5 to 3600 seconds (30 by default)
- You choose what to hide: only the icons, only the cursor, or both
- Settings open from **Settings…** in the tray menu or by double-clicking the tray icon, and apply immediately
- Restores icons and cursor instantly on any mouse or keyboard input, whatever window is active
- Runs quietly in the system tray; **Exit** in the tray menu restores icons and cursor before quitting
- Only one instance can run at a time

## How it works

A WinForms timer ticks every 250 ms. On each tick the app reads the idle time with `GetLastInputInfo` and checks the foreground window with `GetForegroundWindow` + `GetClassName`. The desktop counts as active when the class is `Progman`, `WorkerW` or `Shell_TrayWnd`. Icons are hidden and shown by calling `ShowWindow` on Explorer's `SysListView32` window, found inside `SHELLDLL_DefView`. The cursor is hidden system-wide by replacing the standard cursors with a transparent one via `SetSystemCursor`, and restored by reloading the user's cursor scheme with `SystemParametersInfo(SPI_SETCURSORS)`. The idle threshold and what to hide are taken from the settings; when the user returns, both the icons and the cursor are always shown again. The tray icon is a standard `NotifyIcon`.

## Requirements

- Windows 10 or 11 (tested on Windows 11)
- [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)

## Installation

1. Download `IdleDesktop.exe` from [GitHub Releases](../../releases).
2. Create the folder `%LOCALAPPDATA%\Programs\IdleDesktop` and put the exe there (any folder works, this is just a sensible default).
3. Run it. An icon appears in the system tray.

To start it automatically with Windows:

1. Press `Win+R`, type `shell:startup` and press Enter.
2. Right-click in the folder → **New** → **Shortcut**.
3. Enter the path to `IdleDesktop.exe`.

## Settings

Open **Settings…** from the tray menu (or double-click the tray icon) to set the idle time in seconds and choose what to hide: desktop icons, mouse cursor, or both. Changes made in the window apply immediately.

Settings are stored in `%APPDATA%\IdleDesktop\settings.json`:

```json
{
  "IdleSeconds": 30,
  "Mode": "Both"
}
```

`Mode` is one of `Icons`, `Cursor` or `Both`. If the file is missing or invalid, the defaults (30 seconds, `Both`) are used. An `IdleSeconds` value outside 5–3600 is pulled to the nearest bound. Editing the file by hand takes effect after restarting IdleDesktop.

## Build from source

Requires the .NET 10 SDK. From the repository root, in PowerShell:

```powershell
dotnet publish IdleDesktop -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o "$env:LOCALAPPDATA\Programs\IdleDesktop"
```

To run the tests:

```powershell
dotnet test
```

## Uninstall

1. Choose **Exit** in the tray menu.
2. Delete the shortcut from `shell:startup`, if you created one.
3. Delete the `%LOCALAPPDATA%\Programs\IdleDesktop` folder.
4. Delete the `%APPDATA%\IdleDesktop` folder (your settings).

## Tests

The program's decisions (when to hide and show icons, which windows count as the desktop, and the idle-time calculation that survives the 32-bit tick counter wrapping around) live in pure functions in `IdleLogic` and are covered by xUnit tests. The tests currently cover:

- `IdleLogic`: when to hide and show, which window classes count as the desktop, and the tick-counter wrap-around
- `SettingsStore`: parsing settings (missing, empty or invalid input, unknown mode, clamping of the idle time) and the serialize/parse round trip
- `HideModeExtensions`: which parts each hide mode affects

Everything that talks to Windows (reading system state, hiding icons, the tray icon) is checked manually, since it needs a real desktop. The tests run automatically in GitHub Actions on every push and pull request.

## Known limitations

- Clicking an empty area of the desktop while other windows are open also counts as being "on the desktop".
- If the process is killed forcibly, icons may stay hidden. To restore them, restart Windows Explorer in Task Manager, or start IdleDesktop again and choose **Exit** in the tray menu.
- If the process is killed forcibly (Task Manager, Stop in a debugger), the cursor stays invisible until IdleDesktop is started again (Win → type `IdleDesktop` → Enter) or you sign out and back in.
- Windows only.

## License

[MIT](LICENSE)
