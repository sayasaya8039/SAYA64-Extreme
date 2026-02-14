using System.IO;
using System.Windows;

namespace SAYA64Extreme.Services;

public static class LanguageService
{
    private const string SettingsFileName = "language.conf";
    private static string _currentLanguage = "ja";
    private static ResourceDictionary? _currentStringsDictionary;

    public static string CurrentLanguage => _currentLanguage;

    public static void Initialize()
    {
        var saved = LoadSavedLanguage();
        ApplyLanguage(saved);
    }

    public static void SetLanguage(string lang)
    {
        if (_currentLanguage == lang) return;
        ApplyLanguage(lang);
        SaveLanguage(lang);
    }

    public static string GetString(string key)
    {
        if (Application.Current?.Resources[key] is string s)
            return s;
        return key;
    }

    private static void ApplyLanguage(string lang)
    {
        var uri = lang switch
        {
            "en" => new Uri("Resources/Strings.en.xaml", UriKind.Relative),
            _ => new Uri("Resources/Strings.ja.xaml", UriKind.Relative),
        };

        var dict = new ResourceDictionary { Source = uri };
        var merged = Application.Current.Resources.MergedDictionaries;

        if (_currentStringsDictionary != null)
            merged.Remove(_currentStringsDictionary);

        merged.Add(dict);
        _currentStringsDictionary = dict;
        _currentLanguage = lang;
    }

    private static string LoadSavedLanguage()
    {
        try
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            var path = Path.Combine(dir, SettingsFileName);
            if (File.Exists(path))
            {
                var lang = File.ReadAllText(path).Trim().ToLowerInvariant();
                if (lang is "en" or "ja") return lang;
            }
        }
        catch { }
        return "ja";
    }

    private static void SaveLanguage(string lang)
    {
        try
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            var path = Path.Combine(dir, SettingsFileName);
            File.WriteAllText(path, lang);
        }
        catch { }
    }
}
