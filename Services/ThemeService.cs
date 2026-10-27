namespace Habit_tracker.Services;

public enum ThemeMode
{
    System,
    Light,
    Dark
}

public sealed class ColorPalette
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required Color Accent { get; init; }
    public required Color AccentSoft { get; init; }
    public required Color AccentDark { get; init; }
    public required Color Streak { get; init; }
    public required Color Success { get; init; }
}

public static class ThemeService
{
    const string ModeKey = "theme_mode";
    const string PaletteKey = "color_palette";

    public static readonly IReadOnlyList<ColorPalette> Palettes =
    [
        new()
        {
            Id = "coral",
            Name = "Coral Glow",
            Accent = Color.FromArgb("#FF7A59"),
            AccentSoft = Color.FromArgb("#FFE4DC"),
            AccentDark = Color.FromArgb("#FF9B82"),
            Streak = Color.FromArgb("#FF8A4C"),
            Success = Color.FromArgb("#3DAB7E")
        },
        new()
        {
            Id = "ocean",
            Name = "Ocean Mist",
            Accent = Color.FromArgb("#2BB3C0"),
            AccentSoft = Color.FromArgb("#D4F3F6"),
            AccentDark = Color.FromArgb("#5ECAD4"),
            Streak = Color.FromArgb("#F0A05A"),
            Success = Color.FromArgb("#3DAB7E")
        },
        new()
        {
            Id = "forest",
            Name = "Forest Calm",
            Accent = Color.FromArgb("#5B8C5A"),
            AccentSoft = Color.FromArgb("#E2F0E1"),
            AccentDark = Color.FromArgb("#7BAF7A"),
            Streak = Color.FromArgb("#E08B4C"),
            Success = Color.FromArgb("#4CAF7A")
        },
        new()
        {
            Id = "ink",
            Name = "Ink Night",
            Accent = Color.FromArgb("#6C8CFF"),
            AccentSoft = Color.FromArgb("#E4EAFF"),
            AccentDark = Color.FromArgb("#9AADFF"),
            Streak = Color.FromArgb("#FFB347"),
            Success = Color.FromArgb("#4ADE80")
        }
    ];

    public static ThemeMode CurrentMode { get; private set; } = ThemeMode.System;
    public static ColorPalette CurrentPalette { get; private set; } = Palettes[0];

    public static event EventHandler? ThemeChanged;

    public static void Initialize()
    {
        var modeName = Preferences.Default.Get(ModeKey, nameof(ThemeMode.System));
        CurrentMode = Enum.TryParse(modeName, out ThemeMode mode) ? mode : ThemeMode.System;

        var paletteId = Preferences.Default.Get(PaletteKey, "coral");
        CurrentPalette = Palettes.FirstOrDefault(p => p.Id == paletteId) ?? Palettes[0];

        Apply();
    }

    public static void SetMode(ThemeMode mode)
    {
        CurrentMode = mode;
        Preferences.Default.Set(ModeKey, mode.ToString());
        Apply();
    }

    public static void SetPalette(string paletteId)
    {
        CurrentPalette = Palettes.FirstOrDefault(p => p.Id == paletteId) ?? Palettes[0];
        Preferences.Default.Set(PaletteKey, CurrentPalette.Id);
        Apply();
    }

    public static void Apply()
    {
        if (Application.Current is null)
            return;

        Application.Current.UserAppTheme = CurrentMode switch
        {
            ThemeMode.Light => AppTheme.Light,
            ThemeMode.Dark => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };

        var resources = Application.Current.Resources;
        var p = CurrentPalette;

        SetColor(resources, "Primary", p.Accent);
        SetColor(resources, "PrimaryDark", p.AccentDark);
        SetColor(resources, "Secondary", p.AccentSoft);
        SetColor(resources, "AccentSoft", p.AccentSoft);
        SetColor(resources, "StreakAccent", p.Streak);
        SetColor(resources, "Success", p.Success);
        SetColor(resources, "SuccessLight", p.AccentSoft);
        SetColor(resources, "SuccessDark", p.Success);

        if (resources.TryGetValue("PrimaryBrush", out var pb) && pb is SolidColorBrush primaryBrush)
            primaryBrush.Color = p.Accent;
        if (resources.TryGetValue("SecondaryBrush", out var sb) && sb is SolidColorBrush secondaryBrush)
            secondaryBrush.Color = p.AccentSoft;

        ThemeChanged?.Invoke(null, EventArgs.Empty);
    }

    static void SetColor(ResourceDictionary resources, string key, Color color)
    {
        if (resources.ContainsKey(key))
            resources[key] = color;
        else
            resources.Add(key, color);
    }
}
