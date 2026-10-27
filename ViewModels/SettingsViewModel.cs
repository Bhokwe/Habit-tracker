using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Habit_tracker.Services;

namespace Habit_tracker.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    public ObservableCollection<PaletteOption> Palettes { get; } = [];

    [ObservableProperty] bool isSystemTheme;
    [ObservableProperty] bool isLightTheme;
    [ObservableProperty] bool isDarkTheme;
    [ObservableProperty] string selectedPaletteId = "coral";
    [ObservableProperty] string themeSummary = "";

    public SettingsViewModel()
    {
        foreach (var p in ThemeService.Palettes)
        {
            Palettes.Add(new PaletteOption
            {
                Id = p.Id,
                Name = p.Name,
                Swatch = p.Accent,
                IsSelected = p.Id == ThemeService.CurrentPalette.Id
            });
        }

        RefreshModeFlags();
        SelectedPaletteId = ThemeService.CurrentPalette.Id;
        UpdateSummary();
    }

    [RelayCommand]
    void SetTheme(string mode)
    {
        var themeMode = mode switch
        {
            "Light" => ThemeMode.Light,
            "Dark" => ThemeMode.Dark,
            _ => ThemeMode.System
        };

        ThemeService.SetMode(themeMode);
        RefreshModeFlags();
        UpdateSummary();
    }

    [RelayCommand]
    void SelectPalette(PaletteOption? option)
    {
        if (option is null) return;

        ThemeService.SetPalette(option.Id);
        SelectedPaletteId = option.Id;

        foreach (var p in Palettes)
            p.IsSelected = p.Id == option.Id;

        UpdateSummary();
    }

    void RefreshModeFlags()
    {
        IsSystemTheme = ThemeService.CurrentMode == ThemeMode.System;
        IsLightTheme = ThemeService.CurrentMode == ThemeMode.Light;
        IsDarkTheme = ThemeService.CurrentMode == ThemeMode.Dark;
    }

    void UpdateSummary()
    {
        var mode = ThemeService.CurrentMode switch
        {
            ThemeMode.Light => "Light",
            ThemeMode.Dark => "Dark",
            _ => "Automatic"
        };
        ThemeSummary = $"{mode} · {ThemeService.CurrentPalette.Name}";
    }
}

public partial class PaletteOption : ObservableObject
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public Color Swatch { get; set; } = Colors.Orange;

    [ObservableProperty]
    bool isSelected;
}
