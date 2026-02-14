using System.IO;
using System.Windows;
using Application = System.Windows.Application;

namespace SAYA64Extreme.Services;

public static class ThemeService
{
    private const string SettingsFileName = "theme.conf";
    private static string _currentTheme = "dark";
    private static ResourceDictionary? _currentThemeDictionary;

    public static string CurrentTheme => _currentTheme;

    public static void Initialize()
    {
        var saved = LoadSavedTheme();
        ApplyTheme(saved);
    }

    public static void SetTheme(string theme)
    {
        if (_currentTheme == theme) return;
        ApplyTheme(theme);
        SaveTheme(theme);
    }

    private static void ApplyTheme(string theme)
    {
        var uri = theme switch
        {
            "light" => new Uri("Resources/Theme.Light.xaml", UriKind.Relative),
            _ => new Uri("Resources/Theme.Dark.xaml", UriKind.Relative),
        };

        var dict = new ResourceDictionary { Source = uri };
        var merged = Application.Current.Resources.MergedDictionaries;

        if (_currentThemeDictionary != null)
            merged.Remove(_currentThemeDictionary);

        // テーマは先頭に挿入（Styles.xamlより前に読み込む）
        merged.Insert(0, dict);
        _currentThemeDictionary = dict;
        _currentTheme = theme == "light" ? "light" : "dark";
    }

    private static string LoadSavedTheme()
    {
        try
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            var path = Path.Combine(dir, SettingsFileName);
            if (File.Exists(path))
            {
                var theme = File.ReadAllText(path).Trim().ToLowerInvariant();
                if (theme is "dark" or "light") return theme;
            }
        }
        catch { }
        return "dark";
    }

    private static void SaveTheme(string theme)
    {
        try
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            var path = Path.Combine(dir, SettingsFileName);
            File.WriteAllText(path, theme);
        }
        catch { }
    }
}
