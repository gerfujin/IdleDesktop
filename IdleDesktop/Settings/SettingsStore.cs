using System.Text.Json;
using System.Text.Json.Serialization;

namespace IdleDesktop.Settings;

/// <summary>
/// Reads and writes <see cref="AppSettings"/> as JSON in %APPDATA%\IdleDesktop\settings.json.
/// <see cref="Parse"/> and <see cref="Serialize"/> are pure; <see cref="Load"/> and <see cref="Save"/> touch the file system.
/// </summary>
internal static class SettingsStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "IdleDesktop", "settings.json");

    public static AppSettings Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return AppSettings.Default;

        AppSettings? settings;
        try
        {
            settings = JsonSerializer.Deserialize<AppSettings>(json, Options);
        }
        catch (JsonException)
        {
            return AppSettings.Default;
        }

        // The enum converter also accepts raw numbers, which may not name a real mode.
        if (settings == null || !Enum.IsDefined(settings.Mode))
            return AppSettings.Default;

        return settings with
        {
            IdleSeconds = Math.Clamp(settings.IdleSeconds, AppSettings.MinIdleSeconds, AppSettings.MaxIdleSeconds)
        };
    }

    public static string Serialize(AppSettings settings) => JsonSerializer.Serialize(settings, Options);

    // A missing or unreadable settings file must never stop the app from starting, so errors fall back to defaults.
    public static AppSettings Load()
    {
        try
        {
            return Parse(File.ReadAllText(FilePath));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return AppSettings.Default;
        }
    }

    // Exceptions are not swallowed here: the caller decides how to report a failed save.
    public static void Save(AppSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, Serialize(settings));
    }
}
