# IdleDesktop

A tiny Windows tray utility that hides desktop icons when you sit idle on the desktop and brings them back on any input.

## Features

- Hides desktop icons after 30 seconds of inactivity while the desktop (or taskbar) is the active window
- Restores icons instantly on any mouse or keyboard input, whatever window is active
- Runs quietly in the system tray; **Exit** in the tray menu restores icons before quitting
- Only one instance can run at a time

## How it works

A WinForms timer ticks every 250 ms. On each tick the app reads the idle time with `GetLastInputInfo` and checks the foreground window with `GetForegroundWindow` + `GetClassName`. The desktop counts as active when the class is `Progman`, `WorkerW` or `Shell_TrayWnd`. Icons are hidden and shown by calling `ShowWindow` on Explorer's `SysListView32` window, found inside `SHELLDLL_DefView`. The tray icon is a standard `NotifyIcon`.

## Requirements

- Windows 10 or 11
- [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)

## Installation

1. Download `IdleDesktop.exe` from [GitHub Releases](../../releases).
2. Put it in `%LOCALAPPDATA%\Programs\IdleDesktop`.
3. Run it. An icon appears in the system tray.

To start it automatically with Windows:

1. Press `Win+R`, type `shell:startup` and press Enter.
2. Right-click in the folder → **New** → **Shortcut**.
3. Enter the path to `IdleDesktop.exe`.

## Build from source

Requires the .NET 10 SDK. From the repository root, in PowerShell:

```powershell
dotnet publish IdleDesktop -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o "$env:LOCALAPPDATA\Programs\IdleDesktop"
```

## Uninstall

1. Choose **Exit** in the tray menu.
2. Delete the shortcut from `shell:startup`, if you created one.
3. Delete the `%LOCALAPPDATA%\Programs\IdleDesktop` folder.

## Known limitations

- Clicking an empty area of the desktop while other windows are open also counts as being "on the desktop".
- If the process is killed forcibly, icons may stay hidden. To restore them, restart Windows Explorer in Task Manager, or start IdleDesktop again and choose **Exit** in the tray menu.
- Windows only.

## License

[MIT](LICENSE)
