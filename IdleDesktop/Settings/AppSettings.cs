using System.Text.Json.Serialization;

namespace IdleDesktop.Settings;

/// <summary>
/// User-configurable settings: how long to wait before hiding and what to hide.
/// Persisted by <see cref="SettingsStore"/>.
/// </summary>
internal sealed record AppSettings(int IdleSeconds = 30, HideMode Mode = HideMode.Both)
{
    public const int MinIdleSeconds = 5;
    public const int MaxIdleSeconds = 3600;

    public static AppSettings Default => new(30, HideMode.Both);

    [JsonIgnore]
    public TimeSpan IdleThreshold => TimeSpan.FromSeconds(IdleSeconds);
}
